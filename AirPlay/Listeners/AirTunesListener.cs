using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Plists;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AirPlay.DmapTagged;
using AirPlay.Models;
using AirPlay.Models.Enums;
using AirPlay.Services.Implementations;
using AirPlay.Utils;
using org.whispersystems.curve25519;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace AirPlay.Listeners
{
    public class AirTunesListener : BaseTcpListener
    {
        public const string PAIR_VERIFY_AES_KEY = "Pair-Verify-AES-Key";
        public const string PAIR_VERIFY_AES_IV = "Pair-Verify-AES-IV";

        private readonly IRtspReceiver _receiver;
        private readonly ushort _airTunesPort;
        private readonly ushort _airPlayPort;
        private readonly byte[] _publicKey;
        private readonly byte[] _expandedPrivateKey;
        private readonly byte[] _fpHeader = new byte[] { 0x46, 0x50, 0x4c, 0x59, 0x03, 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, 0x14 };
        private readonly byte[][] _replyMessage = new byte[][]
        {
            new byte[] { 0x46,0x50,0x4c,0x59,0x03,0x01,0x02,0x00,0x00,0x00,0x00,0x82,0x02,0x00,0x0f,0x9f,0x3f,0x9e,0x0a,0x25,0x21,0xdb,0xdf,0x31,0x2a,0xb2,0xbf,0xb2,0x9e,0x8d,0x23,0x2b,0x63,0x76,0xa8,0xc8,0x18,0x70,0x1d,0x22,0xae,0x93,0xd8,0x27,0x37,0xfe,0xaf,0x9d,0xb4,0xfd,0xf4,0x1c,0x2d,0xba,0x9d,0x1f,0x49,0xca,0xaa,0xbf,0x65,0x91,0xac,0x1f,0x7b,0xc6,0xf7,0xe0,0x66,0x3d,0x21,0xaf,0xe0,0x15,0x65,0x95,0x3e,0xab,0x81,0xf4,0x18,0xce,0xed,0x09,0x5a,0xdb,0x7c,0x3d,0x0e,0x25,0x49,0x09,0xa7,0x98,0x31,0xd4,0x9c,0x39,0x82,0x97,0x34,0x34,0xfa,0xcb,0x42,0xc6,0x3a,0x1c,0xd9,0x11,0xa6,0xfe,0x94,0x1a,0x8a,0x6d,0x4a,0x74,0x3b,0x46,0xc3,0xa7,0x64,0x9e,0x44,0xc7,0x89,0x55,0xe4,0x9d,0x81,0x55,0x00,0x95,0x49,0xc4,0xe2,0xf7,0xa3,0xf6,0xd5,0xba},
            new byte[] { 0x46,0x50,0x4c,0x59,0x03,0x01,0x02,0x00,0x00,0x00,0x00,0x82,0x02,0x01,0xcf,0x32,0xa2,0x57,0x14,0xb2,0x52,0x4f,0x8a,0xa0,0xad,0x7a,0xf1,0x64,0xe3,0x7b,0xcf,0x44,0x24,0xe2,0x00,0x04,0x7e,0xfc,0x0a,0xd6,0x7a,0xfc,0xd9,0x5d,0xed,0x1c,0x27,0x30,0xbb,0x59,0x1b,0x96,0x2e,0xd6,0x3a,0x9c,0x4d,0xed,0x88,0xba,0x8f,0xc7,0x8d,0xe6,0x4d,0x91,0xcc,0xfd,0x5c,0x7b,0x56,0xda,0x88,0xe3,0x1f,0x5c,0xce,0xaf,0xc7,0x43,0x19,0x95,0xa0,0x16,0x65,0xa5,0x4e,0x19,0x39,0xd2,0x5b,0x94,0xdb,0x64,0xb9,0xe4,0x5d,0x8d,0x06,0x3e,0x1e,0x6a,0xf0,0x7e,0x96,0x56,0x16,0x2b,0x0e,0xfa,0x40,0x42,0x75,0xea,0x5a,0x44,0xd9,0x59,0x1c,0x72,0x56,0xb9,0xfb,0xe6,0x51,0x38,0x98,0xb8,0x02,0x27,0x72,0x19,0x88,0x57,0x16,0x50,0x94,0x2a,0xd9,0x46,0x68,0x8a},
            new byte[] { 0x46,0x50,0x4c,0x59,0x03,0x01,0x02,0x00,0x00,0x00,0x00,0x82,0x02,0x02,0xc1,0x69,0xa3,0x52,0xee,0xed,0x35,0xb1,0x8c,0xdd,0x9c,0x58,0xd6,0x4f,0x16,0xc1,0x51,0x9a,0x89,0xeb,0x53,0x17,0xbd,0x0d,0x43,0x36,0xcd,0x68,0xf6,0x38,0xff,0x9d,0x01,0x6a,0x5b,0x52,0xb7,0xfa,0x92,0x16,0xb2,0xb6,0x54,0x82,0xc7,0x84,0x44,0x11,0x81,0x21,0xa2,0xc7,0xfe,0xd8,0x3d,0xb7,0x11,0x9e,0x91,0x82,0xaa,0xd7,0xd1,0x8c,0x70,0x63,0xe2,0xa4,0x57,0x55,0x59,0x10,0xaf,0x9e,0x0e,0xfc,0x76,0x34,0x7d,0x16,0x40,0x43,0x80,0x7f,0x58,0x1e,0xe4,0xfb,0xe4,0x2c,0xa9,0xde,0xdc,0x1b,0x5e,0xb2,0xa3,0xaa,0x3d,0x2e,0xcd,0x59,0xe7,0xee,0xe7,0x0b,0x36,0x29,0xf2,0x2a,0xfd,0x16,0x1d,0x87,0x73,0x53,0xdd,0xb9,0x9a,0xdc,0x8e,0x07,0x00,0x6e,0x56,0xf8,0x50,0xce},
            new byte[] { 0x46,0x50,0x4c,0x59,0x03,0x01,0x02,0x00,0x00,0x00,0x00,0x82,0x02,0x03,0x90,0x01,0xe1,0x72,0x7e,0x0f,0x57,0xf9,0xf5,0x88,0x0d,0xb1,0x04,0xa6,0x25,0x7a,0x23,0xf5,0xcf,0xff,0x1a,0xbb,0xe1,0xe9,0x30,0x45,0x25,0x1a,0xfb,0x97,0xeb,0x9f,0xc0,0x01,0x1e,0xbe,0x0f,0x3a,0x81,0xdf,0x5b,0x69,0x1d,0x76,0xac,0xb2,0xf7,0xa5,0xc7,0x08,0xe3,0xd3,0x28,0xf5,0x6b,0xb3,0x9d,0xbd,0xe5,0xf2,0x9c,0x8a,0x17,0xf4,0x81,0x48,0x7e,0x3a,0xe8,0x63,0xc6,0x78,0x32,0x54,0x22,0xe6,0xf7,0x8e,0x16,0x6d,0x18,0xaa,0x7f,0xd6,0x36,0x25,0x8b,0xce,0x28,0x72,0x6f,0x66,0x1f,0x73,0x88,0x93,0xce,0x44,0x31,0x1e,0x4b,0xe6,0xc0,0x53,0x51,0x93,0xe5,0xef,0x72,0xe8,0x68,0x62,0x33,0x72,0x9c,0x22,0x7d,0x82,0x0c,0x99,0x94,0x45,0xd8,0x92,0x46,0xc8,0xc3,0x59}
        };

        public AirTunesListener(IRtspReceiver receiver, ushort port, ushort airPlayPort) : base(port)
        {
            _receiver = receiver;
            _airTunesPort = port;
            _airPlayPort = airPlayPort;

            // First time that we instantiate AirPlayListener we must create a ED25519 KeyPair
            var seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetBytes(seed);
            Chaos.NaCl.Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, seed);

            _publicKey = publicKey;
            _expandedPrivateKey = expandedPrivateKey;
        }

        public override async Task OnDataReceivedAsync(Request request, Response response, CancellationToken cancellationToken)
        {
            // Get session by active-remote header value
            var sessionId = request.Headers["Active-Remote"];
            var session = await SessionManager.Current.GetSessionAsync(sessionId);

            if (request.Type == RequestType.GET && "/info".Equals(request.Path, StringComparison.OrdinalIgnoreCase))
            {
                var dict = new Dictionary<string, object>();
                dict.Add("features", 61379444727);
                dict.Add("name", "airserver");
                dict.Add("displays", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "primaryInputDevice", 1 },
                        { "rotation", true },
                        { "widthPhysical", 0 },
                        { "edid", "AP///////wAGEBOuhXxiyAoaAQS1PCJ4IA8FrlJDsCYOT1QAAAABAQEBAQEBAQEBAQEBAQEBAAAAEAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAAAAAAAAAAAAAAAAA/ABpTWFjCiAgICAgICAgAAAAAAAAAAAAAAAAAAAAAAAAAqBwE3kDAAMAFIBuAYT/E58AL4AfAD8LUQACAAQAf4EY+hAAAQEAEnYx/Hj7/wIQiGLT+vj4/v//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADHkHATeQMAAwFQU+wABP8PnwAvAB8A/whBAAIABABM0AAE/w6fAC8AHwBvCD0AAgAEAMyRAAR/DJ8ALwAfAAcHMwACAAQAVV4ABP8JnwAvAB8AnwUoAAIABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB+Q" },
                        { "widthPixels", 1920.0 },
                        { "uuid", "061013ae-7b0f-4305-984b-974f677a150b" },
                        { "heightPhysical", 0 },
                        { "features", 30 },
                        { "heightPixels", 1080.0 },
                        { "overscanned", false }
                    }
                });
                dict.Add("audioFormats", new List<Dictionary<string, object>>
                {
                    {
                        new Dictionary<string, object> {
                            { "type", 100 },
                            { "audioInputFormats", 67108860 },
                            { "audioOutputFormats", 67108860 }
                        }
                    },
                    {
                        new Dictionary<string, object> {
                            { "type", 101 },
                            { "audioInputFormats", 67108860 },
                            { "audioOutputFormats", 67108860 }
                        }
                    }
                });
                dict.Add("vv", 2);
                dict.Add("statusFlags", 4);
                dict.Add("keepAliveLowPower", true);
                dict.Add("sourceVersion", "220.68");
                dict.Add("pk", "29fbb183a58b466e05b9ab667b3c429d18a6b785637333d3f0f3a34baa89f45c");
                dict.Add("keepAliveSendStatsAsBody", true);
                dict.Add("deviceID", "78:7B:8A:BD:C9:4D");
                dict.Add("model", "AppleTV5,3");
                dict.Add("audioLatencies", new List<Dictionary<string, object>>
                {
                    {
                        new Dictionary<string, object> {
                            { "outputLatencyMicros", 0 },
                            { "type", 100 },
                            { "audioType", "default" },
                            { "inputLatencyMicros", 0 }
                        }
                    },
                    {
                        new Dictionary<string, object> {
                            { "outputLatencyMicros", 0 },
                            { "type", 101 },
                            { "audioType", "default" },
                            { "inputLatencyMicros", 0 }
                        }
                    }
                });
                dict.Add("macAddress", "78:7B:8A:BD:C9:4D");

                var output = default(byte[]);
                using (var outputStream = new MemoryStream())
                {
                    var plistWriter = new BinaryPlistWriter();
                    plistWriter.WriteObject(outputStream, dict, false);
                    outputStream.Seek(0, SeekOrigin.Begin);

                    output = outputStream.ToArray();
                }

                response.Headers.Add("Content-Type", "application/x-apple-binary-plist");
                await response.WriteAsync(output, 0, output.Length).ConfigureAwait(false);
            }
            if (request.Type == RequestType.POST && "/pair-setup".Equals(request.Path, StringComparison.OrdinalIgnoreCase))
            {
                // Return our 32 bytes public key
                response.Headers.Add("Content-Type", "application/octet-stream");
                await response.WriteAsync(_publicKey, 0, _publicKey.Length).ConfigureAwait(false);
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

                        var aesCtr128Encrypt = InitializeChiper(session.EcdhShared);

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

                        var aesCtr128Encrypt = InitializeChiper(session.EcdhShared);

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
            if (request.Type == RequestType.POST && "/fp-setup".Equals(request.Path, StringComparison.OrdinalIgnoreCase))
            {
                // If session is not paired, something gone wrong.
                if (!session.PairCompleted)
                {
                    response.StatusCode = StatusCode.UNAUTHORIZED;
                }
                else
                {
                    var body = request.Body;
                    if (body.Length == 16)
                    {
                        // Response must be 142 bytes
                        var mode = body[14];

                        if (body[4] != 0x03)
                        {
                            // Unsupported fairplay version
                            Console.WriteLine($"Unsupported fairplay version: {body[4]}");
                            return;
                        }

                        // Get mode and send correct reply message
                        mode = body[14];
                        var output = _replyMessage[mode];

                        response.Headers.Add("Content-Type", "application/octet-stream");
                        await response.WriteAsync(output, 0, output.Length);
                    }
                    else if (body.Length == 164)
                    {
                        // Response 32 bytes
                        if (body[4] != 0x03)
                        {
                            // Unsupported fairplay version
                            Console.WriteLine($"Unsupported fairplay version: {body[4]}");
                            return;
                        }

                        var keyMsg = new byte[164];
                        Array.Copy(body, 0, keyMsg, 0, 164);

                        session.KeyMsg = keyMsg;

                        var data = body.Skip(144).ToArray();
                        var output = new byte[32];
                        Array.Copy(_fpHeader, 0, output, 0, 12);
                        Array.Copy(data, 0, output, 12, 20);

                        response.Headers.Add("Content-Type", "application/octet-stream");
                        await response.WriteAsync(output, 0, output.Length);
                    }
                    else
                    {
                        // Unsupported fairplay version
                        Console.WriteLine($"Unsupported fairplay version");
                        return;
                    }
                }
            }
            if (request.Type == RequestType.SETUP)
            {
                // If session is not ready, something gone wrong.
                if (!session.FairPlaySetupCompleted)
                {
                    Console.WriteLine("FairPlay not ready. Something gone wrong.");
                    response.StatusCode = StatusCode.BADREQUEST;
                }
                else
                {
                    var plistReader = new BinaryPlistReader();
                    using (var mem = new MemoryStream(request.Body))
                    {
                        var plist = plistReader.ReadObject(mem);

                        if (plist.Contains("streams"))
                        {
                            // Always one foreach request
                            var stream = (Dictionary<object, object>)((object[])plist["streams"])[0];
                            var type = (short)stream["type"];

                            // If screen Mirroring
                            if (type == 110)
                            {
                                session.StreamConnectionId = unchecked((ulong)(System.Int64)stream["streamConnectionID"]).ToString();

                                // Set video data port
                                var streams = new Dictionary<string, List<Dictionary<string, int>>>()
                                {
                                    {
                                        "streams",
                                        new List<Dictionary<string, int>>
                                        {
                                            {
                                                new Dictionary<string, int>
                                                {
                                                    { "type", 110 },
                                                    { "dataPort", _airPlayPort }
                                                }
                                            }
                                        }
                                    }
                                };

                                byte[] output;
                                using (var outputStream = new MemoryStream())
                                {
                                    var writerRes = new BinaryPlistWriter();
                                    writerRes.WriteObject(outputStream, streams, false);
                                    outputStream.Seek(0, SeekOrigin.Begin);

                                    output = outputStream.ToArray();
                                }

                                response.Headers.Add("Content-Type", "application/x-apple-binary-plist");
                                await response.WriteAsync(output, 0, output.Length).ConfigureAwait(false);
                            }
                            // If audio session
                            if (type == 96)
                            {
                                if (stream.ContainsKey("audioFormat"))
                                {
                                    var audioFormat = (int)stream["audioFormat"];
                                    session.AudioFormat = (AudioFormat)audioFormat;

                                    var description = GetAudioFormatDescription(audioFormat);
                                    Console.WriteLine($"Audio type: {description}");
                                }
                                if (stream.ContainsKey("controlPort"))
                                {
                                    // Use this port to request resend lost packet? (remote port)
                                    var controlPort = (ushort)((short)stream["controlPort"]);

                                }
                                // Set audio data port
                                var streams = new Dictionary<string, List<Dictionary<string, int>>>()
                                {
                                    {
                                        "streams",
                                        new List<Dictionary<string, int>>
                                        {
                                            {
                                                new Dictionary<string, int>
                                                {
                                                    { "type", 96 },
                                                    { "controlPort", 7002 },
                                                    { "dataPort", 7003 }
                                                }
                                            }
                                        }
                                    }
                                };

                                byte[] output;
                                using (var outputStream = new MemoryStream())
                                {
                                    var writerRes = new BinaryPlistWriter();
                                    writerRes.WriteObject(outputStream, streams, false);
                                    outputStream.Seek(0, SeekOrigin.Begin);

                                    output = outputStream.ToArray();
                                }

                                response.Headers.Add("Content-Type", "application/x-apple-binary-plist");
                                await response.WriteAsync(output, 0, output.Length).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            // Read ekey and eiv used to decode video and audio data
                            if (plist.Contains("et"))
                            {
                                var et = (short)plist["et"];
                                Console.WriteLine($"ET: {et}");
                            }
                            if (plist.Contains("ekey"))
                            {
                                session.AesKey = (byte[])plist["ekey"];
                            }
                            if (plist.Contains("eiv"))
                            {
                                session.AesIv = (byte[])plist["eiv"];
                            }
                            if (plist.Contains("isScreenMirroringSession"))
                            {
                                session.MirroringSession = (bool)plist["isScreenMirroringSession"];
                            }
                            if (plist.Contains("timingPort"))
                            {
                                // Use this port to send heartbeat (remote port)
                                var timingPort = (ushort)((short)plist["timingPort"]);
                            }

                            var dict = new Dictionary<string, int>()
                            {
                                { "timingPort", _airTunesPort },
                                { "eventPort", _airTunesPort }
                            };

                            byte[] output;
                            using (var outputStream = new MemoryStream())
                            {
                                var writerRes = new BinaryPlistWriter();
                                writerRes.WriteObject(outputStream, dict, false);
                                outputStream.Seek(0, SeekOrigin.Begin);

                                output = outputStream.ToArray();
                            }

                            response.Headers.Add("Content-Type", "application/x-apple-binary-plist");
                            await response.WriteAsync(output, 0, output.Length).ConfigureAwait(false);
                        }

                        if (session.FairPlayReady && session.MirroringSessionReady && session.MirroringListener == null)
                        {
                            // Start 'MirroringListener' (handle H264 data received from iOS/macOS
                            var mirroring = new MirroringListener(_receiver, session.SessionId, _airPlayPort);
                            await mirroring.StartAsync(cancellationToken).ConfigureAwait(false);

                            session.MirroringListener = mirroring;
                        }
                        if (session.FairPlayReady && session.AudioSessionReady && session.AudioControlListener == null)
                        {
                            // Start 'AudioListener' (handle PCM/AAC/ALAC data received from iOS/macOS
                            var control = new AudioListener(_receiver, session.SessionId, 7002, 7003);
                            await control.StartAsync(cancellationToken).ConfigureAwait(false);

                            session.AudioControlListener = control;
                        }
                    }
                }
            }
            if (request.Type == RequestType.GET_PARAMETER)
            {
                var data = Encoding.ASCII.GetString(request.Body);
                if (data.Equals("volume\r\n"))
                {
                    var output = Encoding.ASCII.GetBytes("volume: 1.000000\r\n");
                    response.Headers.Add("Content-Type", "text/parameters");
                    await response.WriteAsync(output, 0, output.Length).ConfigureAwait(false);
                }
            }
            if (request.Type == RequestType.RECORD)
            {
                response.Headers.Add("Audio-Latency", "0"); // 11025
                // response.Headers.Add("Audio-Jack-Status", "connected; type=analog");
            }
            if (request.Type == RequestType.SET_PARAMETER)
            {
                if (request.Headers.ContainsKey("Content-Type"))
                {
                    var contentType = request.Headers["Content-Type"];

                    if (contentType.Equals("text/parameters", StringComparison.OrdinalIgnoreCase))
                    {
                        var body = Encoding.ASCII.GetString(request.Body);
                        var keyPair = body.Split(":", StringSplitOptions.RemoveEmptyEntries).Select(b => b.Trim(' ', '\r', '\n')).ToArray();
                        if(keyPair.Length == 2)
                        {
                            var key = keyPair[0];
                            var val = keyPair[1];

                            if (key.Equals("volume", StringComparison.OrdinalIgnoreCase))
                            {
                                // request.Body contains 'volume: N.NNNNNN'
                                _receiver.OnSetVolume(decimal.Parse(val));
                            }
                            else if (key.Equals("progress", StringComparison.OrdinalIgnoreCase))
                            {
                                var pVals = val.Split("/", StringSplitOptions.RemoveEmptyEntries);

                                var start = long.Parse(pVals[0]);
                                var current = long.Parse(pVals[1]);
                                var end = long.Parse(pVals[2]);

                                // DO SOMETHING W/ PROGRESS
                            }
                        }
                    }
                    else if (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        var image = request.Body;

                        // DO SOMETHING W/ IMAGE
                    }
                    else if (contentType.Equals("application/x-dmap-tagged", StringComparison.OrdinalIgnoreCase))
                    {
                        var dmap = new DMapTagged();
                        var output = dmap.Decode(request.Body);
                    }
                }
            }
            if(request.Type == RequestType.OPTIONS)
            {
                response.Headers.Add("Public", "SETUP, RECORD, PAUSE, FLUSH, TEARDOWN, OPTIONS, GET_PARAMETER, SET_PARAMETER, ANNOUNCE");
            }
            if(request.Type == RequestType.ANNOUNCE)
            {

            }
            if(request.Type == RequestType.FLUSH)
            {
                int next_seq = -1;

                if (request.Headers.ContainsKey("RTP-Info"))
                {
                    var rtpinfo = request.Headers["RTP-Info"];
                    if (!string.IsNullOrWhiteSpace(rtpinfo))
                    {
                        Console.WriteLine($"Flush with RTP-Info: {rtpinfo}");

                        var r = new Regex(@"seq\=([^;]*)");
                        var m = r.Match(rtpinfo);

                        if (m.Success)
                        {
                            next_seq = int.Parse(m.Groups[1].Value);
                        }
                    }
                }

                await session.AudioControlListener.FlushAsync(next_seq);
            }
            if (request.Type == RequestType.TEARDOWN)
            {
                var plistReader = new BinaryPlistReader();
                using (var mem = new MemoryStream(request.Body))
                {
                    var plist = plistReader.ReadObject(mem);

                    if (plist.Contains("streams"))
                    {
                        // Always one foreach request
                        var stream = (Dictionary<object, object>)((object[])plist["streams"]).Last();
                        var type = (short)stream["type"];

                        // If screen Mirroring
                        if (type == 110)
                        {
                            // Stop mirroring session
                            await session.MirroringListener.StopAsync();
                        }
                        // If audio session
                        if (type == 96)
                        {
                            // Stop audio session
                            await session.AudioControlListener.StopAsync();
                        }
                    }
                }
            }

            // Request w/ path '/feedback' must return 200 OK w/out response body
            // So we can do nothing here..

            // Save current session
            await SessionManager.Current.CreateOrUpdateSessionAsync(sessionId, session);
        }

        private IBufferedCipher InitializeChiper(byte[] ecdhShared)
        {
            var pairverifyaeskey = Encoding.UTF8.GetBytes(PAIR_VERIFY_AES_KEY);
            var pairverifyaesiv = Encoding.UTF8.GetBytes(PAIR_VERIFY_AES_IV);

            byte[] digestAesKey = Utilities.Hash(pairverifyaeskey, ecdhShared);
            byte[] sharedSecretSha512AesKey = Utilities.CopyOfRange(digestAesKey, 0, 16);

            byte[] digestAesIv = Utilities.Hash(pairverifyaesiv, ecdhShared);

            byte[] sharedSecretSha512AesIv = Utilities.CopyOfRange(digestAesIv, 0, 16);

            var aesCipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");

            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", sharedSecretSha512AesKey);
            var cipherParameters = new ParametersWithIV(keyParameter, sharedSecretSha512AesIv, 0, sharedSecretSha512AesIv.Length);

            aesCipher.Init(true, cipherParameters);

            return aesCipher;
        }

        private string GetAudioFormatDescription(int format)
        {
            string formatDescription;

            switch (format)
            {
                case 0x40000:
                    formatDescription = "96 AppleLossless, 96 352 0 16 40 10 14 2 255 0 0 44100";
                    break;
                case 0x400000:
                    formatDescription = "96 mpeg4-generic/44100/2, 96 mode=AAC-main; constantDuration=1024";
                    break;
                case 0x1000000:
                    formatDescription = "96 mpeg4-generic/44100/2, 96 mode=AAC-eld; constantDuration=480";
                    break;
                default:
                    formatDescription = "Unknown: " + format;
                    break;
            }

            return formatDescription;
        }
    }
}
