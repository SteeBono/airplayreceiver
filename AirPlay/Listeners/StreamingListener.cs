using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AirPlay.Models;
using AirPlay.Models.Enums;
using AirPlay.Services.Implementations;
using AirPlay.Utils;
using org.whispersystems.curve25519;

namespace AirPlay.Listeners
{
    public class StreamingListener : BaseTcpListener
    {
        public const string PAIR_VERIFY_AES_KEY = "Pair-Verify-AES-Key";
        public const string PAIR_VERIFY_AES_IV = "Pair-Verify-AES-IV";

        private readonly IRtspReceiver _receiver;
        private readonly string _sessionId;
        private readonly byte[] _expandedPrivateKey;

        public StreamingListener(IRtspReceiver receiver, string sessionId, byte[] expandedPrivateKey, ushort port) : base(port, false)
        {
            _receiver = receiver;
            _sessionId = sessionId;
            _expandedPrivateKey = expandedPrivateKey;
        }

        public override async Task OnDataReceivedAsync(Request request, Response response, CancellationToken cancellationToken)
        {
            // INFO: We must use the same ED25519 KeyPair created inside AirTunesListener
            // INFO: We must use the same sessionId to retrieve 

            var session = await SessionManager.Current.GetSessionAsync(_sessionId);

            if (request.Type == RequestType.GET && "/server-info".Equals(request.Path, StringComparison.OrdinalIgnoreCase))
            {
                var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
                            <plist version=""1.0"">
                             <dict>
                              <key>deviceid</key>
                              <string>10:20:30:40:50:60</string>
                              <key>features</key>
                              <integer>0x5A7FFFF7,0x1E</integer>
                              <key>model</key>
                              <string>AppleTV5,3</string>
                              <key>protovers</key>
                              <string>1.0</string>
                              <key>srcvers</key>
                              <string>220.68</string>
                             </dict>
                            </plist>";

                var bytes = Encoding.ASCII.GetBytes(xml);
                await response.WriteAsync(bytes);
            }
            if (request.Type == RequestType.POST && "/pair-verify".Equals(request.Path, StringComparison.OrdinalIgnoreCase))
            {
                using (var mem = new MemoryStream(request.Body))
                using (var reader = new BinaryReader(mem))
                {
                    // Request: 68 bytes (the first 4 bytes are 01 00 00 00)
                    // Client request packet remaining 64 bytes of content
                    // 01 00 00 00 -> use 01 as flag to check type of verify
                    // If flag is 1:
                    // 32 bytes ecdh_their 
                    // 32 bytes ed_their 
                    // If flag is 0:
                    // 64 bytes signature

                    var flag = reader.ReadByte();
                    if (flag > 0)
                    {
                        reader.ReadBytes(3);
                        session.EcdhTheirs = reader.ReadBytes(32);
                        session.EdTheirs = reader.ReadBytes(32);

                        var curve25519 = Curve25519.getInstance(Curve25519.BEST);
                        var curve25519KeyPair = curve25519.generateKeyPair();

                        session.EcdhOurs = curve25519KeyPair.getPublicKey();
                        var ecdhPrivateKey = curve25519KeyPair.getPrivateKey();

                        session.EcdhShared = curve25519.calculateAgreement(ecdhPrivateKey, session.EcdhTheirs);

                        var aesCtr128Encrypt = Utilities.InitializeChiper(session.EcdhShared);

                        byte[] dataToSign = new byte[64];
                        Array.Copy(session.EcdhOurs, 0, dataToSign, 0, 32);
                        Array.Copy(session.EcdhTheirs, 0, dataToSign, 32, 32);

                        var signature = Chaos.NaCl.Ed25519.Sign(dataToSign, _expandedPrivateKey);

                        byte[] encryptedSignature = aesCtr128Encrypt.DoFinal(signature);

                        byte[] output = new byte[session.EcdhOurs.Length + encryptedSignature.Length];
                        Array.Copy(session.EcdhOurs, 0, output, 0, session.EcdhOurs.Length);
                        Array.Copy(encryptedSignature, 0, output, session.EcdhOurs.Length, encryptedSignature.Length);

                        response.Headers.Add("Content-Type", "application/octet-stream");
                        await response.WriteAsync(output, 0, output.Length).ConfigureAwait(false);
                    }
                    else
                    {
                        reader.ReadBytes(3);
                        var signature = reader.ReadBytes(64);

                        var aesCtr128Encrypt = Utilities.InitializeChiper(session.EcdhShared);

                        var signatureBuffer = new byte[64];
                        signatureBuffer = aesCtr128Encrypt.ProcessBytes(signatureBuffer);
                        signatureBuffer = aesCtr128Encrypt.DoFinal(signature);

                        byte[] messageBuffer = new byte[64];
                        Array.Copy(session.EcdhTheirs, 0, messageBuffer, 0, 32);
                        Array.Copy(session.EcdhOurs, 0, messageBuffer, 32, 32);

                        session.PairVerified = Chaos.NaCl.Ed25519.Verify(signatureBuffer, messageBuffer, session.EdTheirs);

                        Console.WriteLine($"PairVerified: {session.PairVerified}");
                    }
                }
            }
            if (request.Type == RequestType.POST && "/play".Equals(request.Path, StringComparison.OrdinalIgnoreCase))
            {
                using (var mem = new MemoryStream(request.Body))
                using (var reader = new StreamReader(mem, Encoding.ASCII))
                {
                    var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                    var dict = data.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(kv =>
                    {
                        var splitted = kv.Split(": ", StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();
                        return new KeyValuePair<string, string>(splitted[0], splitted[1]);
                    }).ToDictionary(k => k.Key, v => v.Value);

                    var startAt = dict.TryGetValue("Start-Position", out string dStartAt) ? decimal.Parse(dStartAt) : 0M;
                    var url = dict.TryGetValue("Content-Location", out string dUrl) ? dUrl : throw new ArgumentNullException(nameof(dUrl));

                    // DO SOMETHING HERE...

                    var from = 0;

                    using(var client = new HttpClient())
                    {
                        var to = from + 1024;
                        client.DefaultRequestHeaders.Add("Range", $"bytes={from}-{to}");

                        var result = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                        if(result.IsSuccessStatusCode)
                        {
                            var length = result.Headers.TryGetValues("Content-Length", out IEnumerable<string> cLength) ? int.Parse(cLength.FirstOrDefault() ?? "0") : 0;
                            var accept = result.Headers.TryGetValues("Accept-Ranges", out IEnumerable<string> cAccept) ? cAccept.FirstOrDefault() : null;

                            var bytes = await result.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                            _receiver.OnData(new H264Data
                            {
                                Data = bytes
                            });
                        }

                        from = to;
                    }
                }
            }
        }
    }
}
