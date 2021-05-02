using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AirPlay.Models;
using AirPlay.Models.Configs;
using AirPlay.Models.Enums;
using AirPlay.Services.Implementations;
using AirPlay.Utils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace AirPlay.Listeners
{
    public class AudioListener : BaseUdpListener
    {
        public const int RAOP_PACKET_LENGTH = 50000;
        public const int RAOP_BUFFER_LENGTH = 1024; //512;
        public const ulong OFFSET_1900_TO_1970 = 2208988800UL;

        private readonly IRtspReceiver _receiver;
        private readonly string _sessionId;
        private IBufferedCipher _aesCbcDecrypt;
        private readonly OmgHax _omgHax = new OmgHax();

        private IDecoder _decoder;
        private ulong _sync_time;
        private ulong _sync_timestamp;
        private ushort _controlSequenceNumber = 0;
        private RaopBuffer _raopBuffer;
        private Socket _cSocket;

        private readonly CodecLibrariesConfig _config;

        public AudioListener(IRtspReceiver receiver, string sessionId, ushort cport, ushort dport, CodecLibrariesConfig config) : base(cport, dport)
        {
            _receiver = receiver;
            _sessionId = sessionId;
            _config = config;

            _raopBuffer = RaopBufferInit();
            _aesCbcDecrypt = CipherUtilities.GetCipher("AES/CBC/NoPadding");
        }

        public override async Task OnRawCSocketAsync(Socket cSocket, CancellationToken cancellationToken)
        {
            Console.WriteLine("Initializing recevie audio control from socket..");

            _cSocket = cSocket;

            // Get session by active-remove header value
            var session = await SessionManager.Current.GetSessionAsync(_sessionId);

            // If we have not decripted session AesKey
            if (session.DecryptedAesKey == null)
            {
                byte[] decryptedAesKey = new byte[16];
                _omgHax.DecryptAesKey(session.KeyMsg, session.AesKey, decryptedAesKey);
                session.DecryptedAesKey = decryptedAesKey;
            }

            await SessionManager.Current.CreateOrUpdateSessionAsync(_sessionId, session);

            var packet = new byte[RAOP_PACKET_LENGTH];

            do
            {
                var cret = cSocket.Receive(packet, 0, RAOP_PACKET_LENGTH, SocketFlags.None, out SocketError error);
                if(error != SocketError.Success)
                {
                    continue;
                }

                var mem = new MemoryStream(packet);
                using (var reader = new BinaryReader(mem))
                {
                    mem.Position = 1;
                    int type_c = reader.ReadByte() & ~0x80;
                    if (type_c == 0x56)
                    {
                        InitAesCbcCipher(session.DecryptedAesKey, session.EcdhShared, session.AesIv);

                        mem.Position = 4;
                        var data = reader.ReadBytes(cret - 4);

                        var ret = RaopBufferQueue(_raopBuffer, data, (ushort)data.Length, session);
                        if (ret >= 0)
                        {
                            // ERROR
                        }
                    }
                    else if (type_c == 0x54)
                    {
                        /**
                            * packetlen = 20
                            * bytes	description
                            8	RTP header without SSRC
                            8	current NTP time
                            4	RTP timestamp for the next audio packet
                            */

                        mem.Position = 8;
                        ulong ntp_time = (((ulong)reader.ReadInt32()) * 1000000UL) + ((((ulong)reader.ReadInt32()) * 1000000UL) / Int32.MaxValue);
                        uint rtp_timestamp = (uint)((packet[4] << 24) | (packet[5] << 16) | (packet[6] << 8) | packet[7]);
                        uint next_timestamp = (uint)((packet[16] << 24) | (packet[17] << 16) | (packet[18] << 8) | packet[19]);

                        _sync_time = ntp_time - OFFSET_1900_TO_1970 * 1000000UL;
                        _sync_timestamp = rtp_timestamp;
                    }
                    else
                    {
                        Console.WriteLine("Unknown packet");
                    }
                }

                Array.Fill<byte>(packet, 0);
            } while (!cancellationToken.IsCancellationRequested);

            Console.WriteLine("Closing audio control socket..");
        }

        public override async Task OnRawDSocketAsync(Socket dSocket, CancellationToken cancellationToken)
        {
            Console.WriteLine("Initializing recevie audio data from socket..");

            // Get current session
            var session = await SessionManager.Current.GetSessionAsync(_sessionId);

            // If we have not decripted session AesKey
            if (session.DecryptedAesKey == null)
            {
                byte[] decryptedAesKey = new byte[16];
                _omgHax.DecryptAesKey(session.KeyMsg, session.AesKey, decryptedAesKey);
                session.DecryptedAesKey = decryptedAesKey;
            }

            // Initialize decoder
            InitializeDecoder(session.AudioFormat);

            await SessionManager.Current.CreateOrUpdateSessionAsync(_sessionId, session);

            var packet = new byte[RAOP_PACKET_LENGTH];

            do
            {
                var dret = dSocket.Receive(packet, 0, RAOP_PACKET_LENGTH, SocketFlags.None, out SocketError error);
                if (error != SocketError.Success)
                {
                    continue;
                }

                // RTP payload type
                int type_d = packet[1] & ~0x80;

                if (packet.Length >= 12)
                {
                    InitAesCbcCipher(session.DecryptedAesKey, session.EcdhShared, session.AesIv);

                    bool no_resend = false;
                    int buf_ret;
                    byte[] audiobuf;
                    int audiobuflen = 0;
                    uint timestamp = 0;

                    buf_ret = RaopBufferQueue(_raopBuffer, packet, (ushort)dret, session);

                    //if(_raopBuffer.LastSeqNum - _raopBuffer.FirstSeqNum > (RAOP_BUFFER_LENGTH / 8))
                    //{
                        // Dequeue all frames in queue
                        while ((audiobuf = RaopBufferDequeue(_raopBuffer, ref audiobuflen, ref timestamp, no_resend)) != null)
                        {
                            var pcmData = new PcmData();
                            pcmData.Length = 960;
                            pcmData.Data = audiobuf;

                            pcmData.Pts = (ulong)(timestamp - _sync_timestamp) * 1000000UL / 44100 + _sync_time;

                            _receiver.OnPCMData(pcmData);
                        }
                    //}

                    /* Handle possible resend requests */
                    if (!no_resend)
                    {
                        RaopBufferHandleResends(_raopBuffer, _cSocket, _controlSequenceNumber);
                    }
                }

                packet = new byte[RAOP_PACKET_LENGTH];
            } while (!cancellationToken.IsCancellationRequested);

            Console.WriteLine("Closing audio data socket..");
        }

        public Task FlushAsync(int nextSequence)
        {
            RaopBufferFlush(_raopBuffer, nextSequence);
            return Task.CompletedTask;
        }

        private void InitAesCbcCipher(byte[] aesKey, byte[] ecdhShared, byte[] aesIv)
        {
            byte[] hash = Utilities.Hash(aesKey, ecdhShared);
            byte[] eaesKey = Utilities.CopyOfRange(hash, 0, 16);

            var keyParameter = ParameterUtilities.CreateKeyParameter("AES", eaesKey);
            var cipherParameters = new ParametersWithIV(keyParameter, aesIv, 0, aesIv.Length);

            _aesCbcDecrypt.Init(false, cipherParameters);
        }

        private RaopBuffer RaopBufferInit()
        {
            var audio_buffer_size = 480 * 4;
            var raop_buffer = new RaopBuffer();

            raop_buffer.BufferSize = audio_buffer_size * RAOP_BUFFER_LENGTH;
            raop_buffer.Buffer = new byte[raop_buffer.BufferSize];

            for (int i=0; i < RAOP_BUFFER_LENGTH; i++) {
		        var entry = raop_buffer.Entries[i];
                entry.AudioBufferSize = audio_buffer_size;
		        entry.AudioBufferLen = 0;
		        entry.AudioBuffer = (byte[]) raop_buffer.Buffer.Skip(i).Take(audio_buffer_size).ToArray();

                raop_buffer.Entries[i] = entry;
            }

            raop_buffer.IsEmpty = true;

	        return raop_buffer;
        }

        public int RaopBufferQueue(RaopBuffer raop_buffer, byte[] data, ushort datalen, Session session)
        {
            int encryptedlen;
            RaopBufferEntry entry;

            /* Check packet data length is valid */
            if (datalen < 12 || datalen > RAOP_PACKET_LENGTH)
            {
                return -1;
            }

            var seqnum = (ushort)((data[2] << 8) | data[3]);
            if (datalen == 16 && data[12] == 0x0 && data[13] == 0x68 && data[14] == 0x34 && data[15] == 0x0)
            {
                return 0;
            }

            // Ignore, old
            if (!raop_buffer.IsEmpty && seqnum < raop_buffer.FirstSeqNum && seqnum != 0)
            {
                return 0;
            }

            /* Check that there is always space in the buffer, otherwise flush */
            if (raop_buffer.FirstSeqNum + RAOP_BUFFER_LENGTH < seqnum || seqnum == 0)
            {
                RaopBufferFlush(raop_buffer, seqnum);
            }

            entry = raop_buffer.Entries[seqnum % RAOP_BUFFER_LENGTH];
            if (entry.Available && entry.SeqNum == seqnum)
            {
                /* Packet resent, we can safely ignore */
                return 0;
            }

            entry.Flags = data[0];
            entry.Type = data[1];
            entry.SeqNum = seqnum;

            entry.TimeStamp = (uint)((data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7]);
            entry.SSrc = (uint)((data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11]);
            entry.Available = true;

            int payloadsize = datalen - 12;
            var raw = new byte[payloadsize];

            encryptedlen = payloadsize / 16 * 16;

            if (encryptedlen > 0)
            {
                _aesCbcDecrypt.ProcessBytes(data, 12, encryptedlen, data, 12);
                Array.Copy(data, 12, raw, 0, encryptedlen);
            }

            Array.Copy(data, 12 + encryptedlen, raw, encryptedlen, payloadsize - encryptedlen);

#if DUMP
            /* RAW -> DUMP */
            File.WriteAllBytes($"/Users/steebono/Desktop/dump/frames/raw_{seqnum}", raw);
#endif
            /* RAW -> PCM */
            var length = _decoder.GetOutputStreamLength();
            var output = new byte[length];

            var res = _decoder.DecodeFrame(raw, ref output, length);
            if (res != 0)
            {
                output = new byte[length];
                Console.WriteLine($"Decoding error. Decoder: {_decoder.Type} Code: {res}");
            }

#if DUMP
            Console.WriteLine($"RES: {res}");
            Console.WriteLine($"PCM: {output.Length}");
            Console.WriteLine($"LNG: {length}");
            File.WriteAllBytes($"/Users/steebono/Desktop/dump/pcm/raw_{seqnum}", output);
#endif
            Array.Copy(output, 0, entry.AudioBuffer, 0, output.Length);
            entry.AudioBufferLen = output.Length;

            /* Update the raop_buffer seqnums */
            if (raop_buffer.IsEmpty)
            {
                raop_buffer.FirstSeqNum = seqnum;
                raop_buffer.LastSeqNum = seqnum;
                raop_buffer.IsEmpty = false;
            }

            if (raop_buffer.LastSeqNum < seqnum)
            {
                raop_buffer.LastSeqNum = seqnum;
            }

            // Update entries
            raop_buffer.Entries[seqnum % RAOP_BUFFER_LENGTH] = entry;

            return 1;
        }

        public byte[] RaopBufferDequeue(RaopBuffer raop_buffer, ref int length, ref uint pts, bool noResend)
        {
            short buflen;
            RaopBufferEntry entry;

            /* Calculate number of entries in the current buffer */
            buflen = (short)(raop_buffer.LastSeqNum - raop_buffer.FirstSeqNum + 1);

            /* Cannot dequeue from empty buffer */
            if (raop_buffer.IsEmpty || buflen <= 0)
            {
                return null;
            }

            /* Get the first buffer entry for inspection */
            entry = raop_buffer.Entries[raop_buffer.FirstSeqNum % RAOP_BUFFER_LENGTH];
            if (noResend)
            {
                /* If we do no resends, always return the first entry */
                entry.Available = false;

                /* Return entry audio buffer */
                length = entry.AudioBufferLen;
                pts = entry.TimeStamp;
                entry.AudioBufferLen = 0;

                raop_buffer.Entries[raop_buffer.FirstSeqNum % RAOP_BUFFER_LENGTH] = entry;
                raop_buffer.FirstSeqNum += 1;

                return entry.AudioBuffer;
            }
            else if (!entry.Available)
            {
                /* Check how much we have space left in the buffer */
                if (buflen < RAOP_BUFFER_LENGTH)
                {
                    /* Return nothing and hope resend gets on time */
                    length = entry.AudioBufferSize;
                    Array.Fill<byte>(entry.AudioBuffer, 0, 0, length);

                    return entry.AudioBuffer;
                }
                /* Risk of buffer overrun, return empty buffer */
                return Array.Empty<byte>();
            }

            /* Update buffer and validate entry */
            if (!entry.Available)
            {
                /* Return an empty audio buffer to skip audio */
                length = entry.AudioBufferSize;
                Array.Fill<byte>(entry.AudioBuffer, 0, 0, length);

                return entry.AudioBuffer;
            }
            entry.Available = false;

            /* Return entry audio buffer */
            length = entry.AudioBufferLen;
            pts = entry.TimeStamp;
            entry.AudioBufferLen = 0;

            raop_buffer.Entries[raop_buffer.FirstSeqNum % RAOP_BUFFER_LENGTH] = entry;
            raop_buffer.FirstSeqNum += 1;

            return entry.AudioBuffer.Take(length).ToArray();
        }

        private void RaopBufferFlush(RaopBuffer raop_buffer, int next_seq)
        {
            int i;
            for (i = 0; i < RAOP_BUFFER_LENGTH; i++)
            {
                raop_buffer.Entries[i].Available = false;
                raop_buffer.Entries[i].AudioBufferLen = 0;
            }
            if (next_seq < 0 || next_seq > 0xffff)
            {
                raop_buffer.IsEmpty = true;
            }
            else
            {
                raop_buffer.FirstSeqNum = (ushort)next_seq;
                raop_buffer.LastSeqNum = (ushort)(next_seq - 1);
            }
        }

        private void RaopBufferHandleResends(RaopBuffer raop_buffer, Socket cSocket, ushort control_seqnum)
        {
            RaopBufferEntry entry;

            if (Utilities.SeqNumCmp(raop_buffer.FirstSeqNum, raop_buffer.LastSeqNum) < 0)
            {
                int seqnum, count;

                for (seqnum = raop_buffer.FirstSeqNum; Utilities.SeqNumCmp(seqnum, raop_buffer.LastSeqNum) < 0; seqnum++)
                {
                    entry = raop_buffer.Entries[seqnum % RAOP_BUFFER_LENGTH];
                    if (entry.Available)
                    {
                        break;
                    }
                }
                if (Utilities.SeqNumCmp(seqnum, raop_buffer.FirstSeqNum) == 0)
                {
                    return;
                }
                count = Utilities.SeqNumCmp(seqnum, raop_buffer.FirstSeqNum);
                RaopRtpResendCallback(cSocket, control_seqnum, raop_buffer.FirstSeqNum, (ushort)count);
            }
        }

        private int RaopRtpResendCallback(Socket cSocket, ushort control_seqnum, ushort seqnum, ushort count)
        {
            var packet = new byte[8];
            ushort ourseqnum;

            int ret;
            ourseqnum = control_seqnum++;

            /* Fill the request buffer */
            packet[0] = 0x80;
            packet[1] = 0x55|0x80;
            packet[2] = (byte)(ourseqnum >> 8);
            packet[3] = (byte)ourseqnum;
            packet[4] = (byte)(seqnum >> 8);
            packet[5] = (byte)seqnum;
            packet[6] = (byte)(count >> 8);
            packet[7] = (byte)count;

            ret = cSocket.Send(packet, 0, packet.Length, SocketFlags.None);
            if (ret == -1) {
                Console.WriteLine("Resend packet - failed to send request");
            }

            return 0;
        }

        private void InitializeDecoder (AudioFormat audioFormat)
        {
            if (_decoder != null) return;

            if (audioFormat == AudioFormat.ALAC)
            {
                // RTP info: 96 AppleLossless, 96 352 0 16 40 10 14 2 255 0 0 44100
                // (ALAC -> PCM)

                var frameLength = 352;
                var numChannels = 2;
                var bitDepth = 16;
                var sampleRate = 44100;

                _decoder = new ALACDecoder(_config.ALACLibPath);
                _decoder.Config(sampleRate, numChannels, bitDepth, frameLength);
            }
            else if (audioFormat == AudioFormat.AAC)
            {
                // RTP info: 96 mpeg4-generic/44100/2, 96 mode=AAC-main; constantDuration=1024
                // (AAC-MAIN -> PCM)

                var frameLength = 1024;
                var numChannels = 2;
                var bitDepth = 16;
                var sampleRate = 44100;

                _decoder = new AACDecoder(_config.AACLibPath, TransportType.TT_MP4_RAW, AudioObjectType.AOT_AAC_MAIN, 1);
                _decoder.Config(sampleRate, numChannels, bitDepth, frameLength);
            }
            else if(audioFormat == AudioFormat.AAC_ELD)
            {
                // RTP info: 96 mpeg4-generic/44100/2, 96 mode=AAC-eld; constantDuration=480
                // (AAC-ELD -> PCM)

                var frameLength = 480;
                var numChannels = 2;
                var bitDepth = 16;
                var sampleRate = 44100;

                _decoder = new AACDecoder(_config.AACLibPath, TransportType.TT_MP4_RAW, AudioObjectType.AOT_ER_AAC_ELD, 1);
                _decoder.Config(sampleRate, numChannels, bitDepth, frameLength);
            }
            else
            {
                // (PCM -> PCM)
                // Not used
                _decoder = new PCMDecoder();
            }
        }
    }
}
