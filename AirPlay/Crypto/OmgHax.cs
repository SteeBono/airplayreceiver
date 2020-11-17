using System;
using System.IO;
using System.Linq;
using AirPlay.Utils;

namespace AirPlay
{
    public class OmgHax
    {
        private ModifiedMD5 _modifiedMD5 = new ModifiedMD5();
        private SapHash _sapHash = new SapHash();

        public void DecryptAesKey(byte[] message3, byte[] cipherText, byte[] keyOut)
        {
            var chunk1 = Utilities.CopyOfRange(cipherText, 16, cipherText.Length);
            var chunk2 = Utilities.CopyOfRange(cipherText, 56, cipherText.Length);

            var blockIn = new byte[16];
            var sapKey = new byte[16];
            var keySchedule = new int[11][];

            GenerateSessionKey(OmgHaxHex.DefaultSap, message3, sapKey);
            GenerateKeySchedule(sapKey, keySchedule);

            ZXor(chunk2, blockIn, 1);
            Cycle(blockIn, keySchedule);

            for (int i = 0; i < 16; i++)
            {
                keyOut[i] = (byte) (blockIn[i] ^ chunk1[i]);
            }

            XXor(keyOut, keyOut, 1);

            ZXor(keyOut, keyOut, 1);
        }

        private void DecryptMessage(byte[] messageIn, byte[] decryptedMessage)
        {
            var buffer = new byte[16];
            byte tmp;

            int mode = messageIn[12];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (mode == 3)
                    {
                        buffer[j] = messageIn[(0x80 - 0x10 * i) + j];
                    }
                    else if (mode == 2 || mode == 1 || mode == 0)
                    {
                        buffer[j] = messageIn[(0x10 * (i + 1)) + j];
                    }
                }

                for (int j = 0; j < 9; j++)
                {
                    int @base = 0x80 - 0x10 * j;

                    buffer[0x0] = (byte) (MessageTableIndex(@base + 0x0)[buffer[0x0] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x0]);
                    buffer[0x4] = (byte) (MessageTableIndex(@base + 0x4)[buffer[0x4] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x4]);
                    buffer[0x8] = (byte) (MessageTableIndex(@base + 0x8)[buffer[0x8] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x8]);
                    buffer[0xc] = (byte) (MessageTableIndex(@base + 0xc)[buffer[0xc] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0xc]);

                    tmp = buffer[0x0d];
                    buffer[0xd] = (byte) (MessageTableIndex(@base + 0xd)[buffer[0x9] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0xd]);
                    buffer[0x9] = (byte) (MessageTableIndex(@base + 0x9)[buffer[0x5] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x9]);
                    buffer[0x5] = (byte) (MessageTableIndex(@base + 0x5)[buffer[0x1] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x5]);
                    buffer[0x1] = (byte) (MessageTableIndex(@base + 0x1)[tmp & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x1]);

                    tmp = buffer[0x02];
                    buffer[0x2] = (byte) (MessageTableIndex(@base + 0x2)[buffer[0xa] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x2]);
                    buffer[0xa] = (byte) (MessageTableIndex(@base + 0xa)[tmp & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0xa]);
                    tmp = buffer[0x06];
                    buffer[0x6] = (byte) (MessageTableIndex(@base + 0x6)[buffer[0xe] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x6]);
                    buffer[0xe] = (byte) (MessageTableIndex(@base + 0xe)[tmp & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0xe]);

                    tmp = buffer[0x3];
                    buffer[0x3] = (byte) (MessageTableIndex(@base + 0x3)[buffer[0x7] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x3]);
                    buffer[0x7] = (byte) (MessageTableIndex(@base + 0x7)[buffer[0xb] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0x7]);
                    buffer[0xb] = (byte) (MessageTableIndex(@base + 0xb)[buffer[0xf] & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0xb]);
                    buffer[0xf] = (byte) (MessageTableIndex(@base + 0xf)[tmp & 0xFF] ^ OmgHaxHex.MessageKey[mode][@base + 0xf]);

                    using (var mem = new MemoryStream(buffer))
                    using (var writer = new BinaryWriter(mem))
                    {
                        writer.Write(OmgHaxHex.TableS9[0x000 + (buffer[0x0] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x100 + (buffer[0x1] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x200 + (buffer[0x2] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x300 + (buffer[0x3] & 0xFF)]);
                        writer.Write(OmgHaxHex.TableS9[0x000 + (buffer[0x4] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x100 + (buffer[0x5] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x200 + (buffer[0x6] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x300 + (buffer[0x7] & 0xFF)]);
                        writer.Write(OmgHaxHex.TableS9[0x000 + (buffer[0x8] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x100 + (buffer[0x9] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x200 + (buffer[0xa] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x300 + (buffer[0xb] & 0xFF)]);
                        writer.Write(OmgHaxHex.TableS9[0x000 + (buffer[0xc] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x100 + (buffer[0xd] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x200 + (buffer[0xe] & 0xFF)] ^
                                OmgHaxHex.TableS9[0x300 + (buffer[0xf] & 0xFF)]);
                    }
                }

                buffer[0x0] = OmgHaxHex.TableS10[(0x0 << 8) + (buffer[0x0] & 0xFF)];
                buffer[0x4] = OmgHaxHex.TableS10[(0x4 << 8) + (buffer[0x4] & 0xFF)];
                buffer[0x8] = OmgHaxHex.TableS10[(0x8 << 8) + (buffer[0x8] & 0xFF)];
                buffer[0xc] = OmgHaxHex.TableS10[(0xc << 8) + (buffer[0xc] & 0xFF)];

                tmp = buffer[0x0d];
                buffer[0xd] = OmgHaxHex.TableS10[(0xd << 8) + (buffer[0x9] & 0xFF)];
                buffer[0x9] = OmgHaxHex.TableS10[(0x9 << 8) + (buffer[0x5] & 0xFF)];
                buffer[0x5] = OmgHaxHex.TableS10[(0x5 << 8) + (buffer[0x1] & 0xFF)];
                buffer[0x1] = OmgHaxHex.TableS10[(0x1 << 8) + (tmp & 0xFF)];

                tmp = buffer[0x02];
                buffer[0x2] = OmgHaxHex.TableS10[(0x2 << 8) + (buffer[0xa] & 0xFF)];
                buffer[0xa] = OmgHaxHex.TableS10[(0xa << 8) + (tmp & 0xFF)];
                tmp = buffer[0x06];
                buffer[0x6] = OmgHaxHex.TableS10[(0x6 << 8) + (buffer[0xe] & 0xFF)];
                buffer[0xe] = OmgHaxHex.TableS10[(0xe << 8) + (tmp & 0xFF)];

                tmp = buffer[0x3];
                buffer[0x3] = OmgHaxHex.TableS10[(0x3 << 8) + (buffer[0x7] & 0xFF)];
                buffer[0x7] = OmgHaxHex.TableS10[(0x7 << 8) + (buffer[0xb] & 0xFF)];
                buffer[0xb] = OmgHaxHex.TableS10[(0xb << 8) + (buffer[0xf] & 0xFF)];
                buffer[0xf] = OmgHaxHex.TableS10[(0xf << 8) + (tmp & 0xFF)];

                var xorResult = new byte[16];
                if (mode == 2 || mode == 1 || mode == 0)
                {
                    if (i > 0)
                    {
                        XorBlocks(buffer, Utilities.CopyOfRange(messageIn, 0x10 * i, 0x10 * i + 16), xorResult);
                        Array.Copy(xorResult, 0, decryptedMessage, 0x10 * i, 16);
                    }
                    else
                    {
                        XorBlocks(buffer, OmgHaxHex.MessageIv[mode], xorResult);
                        Array.Copy(xorResult, 0, decryptedMessage, 0x10 * i, 16);
                    }

                }
                else
                {
                    if (i < 7)
                    {
                        XorBlocks(buffer, Utilities.CopyOfRange(messageIn, 0x70 - 0x10 * i, (0x70 - 0x10 * i) + 16), xorResult);
                        Array.Copy(xorResult, 0, decryptedMessage, 0x70 - 0x10 * i, 16);
                    }
                    else
                    {
                        XorBlocks(buffer, OmgHaxHex.MessageIv[mode], xorResult);
                        Array.Copy(xorResult, 0, decryptedMessage, 0x70 - 0x10 * i, 16);
                    }
                }
            }
        }

        private void GenerateKeySchedule(byte[] key_material, int[][] key_schedule)
        {
            var key_data = new int[4];
            var deadbeef = 0xdeadbeef;

            for (int i = 0; i < 11; i++)
            {
                key_schedule[i] = new int[4] { (byte)deadbeef, (byte)deadbeef, (byte)deadbeef, (byte)deadbeef };
            }

            var buffer = new byte[16];
            var ti = 0;

            TXor(key_material, buffer);

            using (var mem = new MemoryStream(buffer))
            using (var reader = new BinaryReader(mem))
            using (var writer = new BinaryWriter(mem))
            {
                for (int i = 0; i < 4; i++)
                {
                    key_data[i] = reader.ReadInt32();
                }

                for (int round = 0; round < 11; round++)
                {
                    key_schedule[round][0] = key_data[0];

                    byte[] table1 = TableIndex(ti);
                    byte[] table2 = TableIndex(ti + 1);
                    byte[] table3 = TableIndex(ti + 2);
                    byte[] table4 = TableIndex(ti + 3);
                    ti += 4;

                    buffer[0] ^= (byte)(table1[buffer[0x0d] & 0xFF] ^ OmgHaxHex.IndexMAngle[round]);
                    buffer[1] ^= table2[buffer[0x0e] & 0xFF];
                    buffer[2] ^= table3[buffer[0x0f] & 0xFF];
                    buffer[3] ^= table4[buffer[0x0c] & 0xFF];

                    mem.Position = 0;
                    key_data[0] = reader.ReadInt32();
                    key_schedule[round][1] = key_data[1];
                    key_data[1] ^= key_data[0];

                    mem.Position = 4;
                    writer.Write(key_data[1]);
                    key_schedule[round][2] = key_data[2];
                    key_data[2] ^= key_data[1];

                    mem.Position = 8;
                    writer.Write(key_data[2]);
                    key_schedule[round][3] = key_data[3];
                    key_data[3] ^= key_data[2];

                    mem.Position = 12;
                    writer.Write(key_data[3]);
                }

                for (int i = 0; i < 11; i++)
                {
                    var tmp = new byte[16];
                    using (var tmpMem = new MemoryStream(tmp))
                    using (var tmpWriter = new BinaryWriter(tmpMem))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            tmpWriter.Write(key_schedule[i][j]);
                        }
                    }
                }
            }
        }

        private void GenerateSessionKey(byte[] oldSap, byte[] messageIn, byte[] sessionKey)
        {
            var decryptedMessage = new byte[128];
            var newSap = new byte[320];
            var md5 = new byte[16];

            DecryptMessage(messageIn, decryptedMessage);

            Array.Copy(OmgHaxHex.StaticSource1, 0, newSap, 0, 0x11);
            Array.Copy(decryptedMessage, 0, newSap, 0x11, 0x80);
            Array.Copy(oldSap, 0x80, newSap, 0x091, 0x80);
            Array.Copy(OmgHaxHex.StaticSource2, 0, newSap, 0x111, 0x2f);
            Array.Copy(OmgHaxHex.InitialSessionKey, 0, sessionKey, 0, 16);

            for (var round = 0; round < 5; round++)
            {
                var @base = Utilities.CopyOfRange(newSap, round * 64, newSap.Length);
                _modifiedMD5.ModifiedMd5(@base, sessionKey, md5);
                _sapHash.Hash(@base, sessionKey);

                using (var md5Mem = new MemoryStream(md5))
                using (var sessionKeyMem = new MemoryStream(sessionKey))
                using (var md5Reader = new BinaryReader(md5Mem))
                using (var sessionKeyReader = new BinaryReader(sessionKeyMem))
                using (var sessionKeyWriter = new BinaryWriter(sessionKeyMem))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        sessionKeyMem.Position = i * 4;
                        var intSess = sessionKeyReader.ReadInt32();

                        md5Mem.Position = i * 4;
                        var intMd5 = md5Reader.ReadInt32();

                        var pos = i * 4;
                        var data = (int)((intSess + intMd5) & 0xffffffffL);

                        sessionKeyMem.Position = pos;
                        sessionKeyWriter.Write(data);
                    }
                }
            }

            for (int i = 0; i < 16; i += 4)
            {
                byte tmp = sessionKey[i];

                sessionKey[i] = sessionKey[i + 3];
                sessionKey[i + 3] = tmp;

                tmp = sessionKey[i + 1];

                sessionKey[i + 1] = sessionKey[i + 2];
                sessionKey[i + 2] = tmp;
            }

            for (int i = 0; i < 16; i++)
            {
                sessionKey[i] ^= 121;
            }
        }

        private void Cycle(byte[] block, int[][] key_schedule)
        {
            int ptr1, ptr2, ptr3, ptr4, ab;

            using (var mem = new MemoryStream(block))
            using (var reader = new BinaryReader(mem))
            using (var writer = new BinaryWriter(mem))
            {
                mem.Position = 0;
                var d1 = reader.ReadInt32() ^ key_schedule[10][0];
                mem.Position = 0;
                writer.Write(d1);

                mem.Position = 4;
                var d2 = reader.ReadInt32() ^ key_schedule[10][1];
                mem.Position = 4;
                writer.Write(d2);

                mem.Position = 8;
                var d3 = reader.ReadInt32() ^ key_schedule[10][2];
                mem.Position = 8;
                writer.Write(d3);

                mem.Position = 12;
                var d4 = reader.ReadInt32() ^ key_schedule[10][3];
                mem.Position = 12;
                writer.Write(d4);

                PermuteBlock1(block);

                for (int round = 0; round < 9; round++)
                {
                    var key = new byte[16];
                    var keyMem = new MemoryStream(key);

                    using (var keyWriter = new BinaryWriter(keyMem))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            keyWriter.Write(key_schedule[9 - round][i]);
                        }
                    }

                    ptr1 = OmgHaxHex.TableS5[(block[3] & 0xff) ^ (key[3] & 0xff)];
                    ptr2 = OmgHaxHex.TableS6[(block[2] & 0xff) ^ (key[2] & 0xff)];
                    ptr3 = OmgHaxHex.TableS8[(block[0] & 0xff) ^ (key[0] & 0xff)];
                    ptr4 = OmgHaxHex.TableS7[(block[1] & 0xff) ^ (key[1] & 0xff)];

                    ab = ptr1 ^ ptr2 ^ ptr3 ^ ptr4;

                    mem.Position = 0;
                    writer.Write(ab);

                    ptr2 = OmgHaxHex.TableS5[(block[7] & 0xff) ^ (key[7] & 0xff)];
                    ptr1 = OmgHaxHex.TableS6[(block[6] & 0xff) ^ (key[6] & 0xff)];
                    ptr4 = OmgHaxHex.TableS7[(block[5] & 0xff) ^ (key[5] & 0xff)];
                    ptr3 = OmgHaxHex.TableS8[(block[4] & 0xff) ^ (key[4] & 0xff)];

                    ab = ptr1 ^ ptr2 ^ ptr3 ^ ptr4;

                    mem.Position = 4;
                    writer.Write(ab);

                    mem.Position = 8;
                    writer.Write(OmgHaxHex.TableS5[(block[11] & 0xff) ^ (key[11] & 0xff)] ^
                            OmgHaxHex.TableS6[(block[10] & 0xff) ^ (key[10] & 0xff)] ^
                            OmgHaxHex.TableS7[(block[9] & 0xff) ^ (key[9] & 0xff)] ^
                            OmgHaxHex.TableS8[(block[8] & 0xff) ^ (key[8] & 0xff)]);

                    mem.Position = 12;
                    writer.Write(OmgHaxHex.TableS5[(block[15] & 0xff) ^ (key[15] & 0xff)] ^
                            OmgHaxHex.TableS6[(block[14] & 0xff) ^ (key[14] & 0xff)] ^
                            OmgHaxHex.TableS7[(block[13] & 0xff) ^ (key[13] & 0xff)] ^
                            OmgHaxHex.TableS8[(block[12] & 0xff) ^ (key[12] & 0xff)]);

                    PermuteBlock2(block, 8 - round);
                }

                mem.Position = 0;
                var r1 = reader.ReadInt32() ^ key_schedule[0][0];
                mem.Position = 0;
                writer.Write(r1);

                mem.Position = 4;
                var r2 = reader.ReadInt32() ^ key_schedule[0][1];
                mem.Position = 4;
                writer.Write(r2);

                mem.Position = 8;
                var r3 = reader.ReadInt32() ^ key_schedule[0][2];
                mem.Position = 8;
                writer.Write(r3);

                mem.Position = 12;
                var r4 = reader.ReadInt32() ^ key_schedule[0][3];
                mem.Position = 12;
                writer.Write(r4);
            }
        }

        private void XorBlocks(byte[] a, byte[] b, byte[] @out)
        {
            for (int i = 0; i < 16; i++)
            {
                @out[i] = (byte) (a[i] ^ b[i]);
            }
        }

        private void ZXor(byte[] @in, byte[] @out, int blocks)
        {
            for (int j = 0; j < blocks; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    @out[j * 16 + i] = (byte) (@in[j * 16 + i] ^ OmgHaxHex.ZKey[i]);
                }
            }
        }

        private void XXor(byte[] @in, byte[] @out, int blocks)
        {
            for (int j = 0; j < blocks; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    @out[j * 16 + i] = (byte) (@in[j * 16 + i] ^ OmgHaxHex.XKey[i]);
                }
            }
        }

        private void TXor(byte[] @in, byte[] @out)
        {
            for (int i = 0; i < 16; i++)
            {
                @out[i] = (byte) (@in[i] ^ OmgHaxHex.TKey[i]);
            }
        }

        private byte[] TableIndex(int i)
        {
            return Utilities.CopyOfRange(OmgHaxHex.TableS1, ((31 * i) % 0x28) << 8, OmgHaxHex.TableS1.Length);
        }

        private byte[] MessageTableIndex(int i)
        {
            return Utilities.CopyOfRange(OmgHaxHex.TableS2, (97 * i % 144) << 8, OmgHaxHex.TableS2.Length);
        }

        private void PermuteBlock1(byte[] block)
        {
            block[0] = OmgHaxHex.TableS3[block[0] & 0xff];
            block[4] = OmgHaxHex.TableS3[0x400 + (block[4] & 0xff)];
            block[8] = OmgHaxHex.TableS3[0x800 + (block[8] & 0xff)];
            block[12] = OmgHaxHex.TableS3[0xc00 + (block[12] & 0xff)];

            byte tmp = block[13];
            block[13] = OmgHaxHex.TableS3[0x100 + (block[9] & 0xff)];
            block[9] = OmgHaxHex.TableS3[0xd00 + (block[5] & 0xff)];
            block[5] = OmgHaxHex.TableS3[0x900 + (block[1] & 0xff)];
            block[1] = OmgHaxHex.TableS3[0x500 + (tmp & 0xff)];

            tmp = block[2];
            block[2] = OmgHaxHex.TableS3[0xa00 + (block[10] & 0xff)];
            block[10] = OmgHaxHex.TableS3[0x200 + (tmp & 0xff)];
            tmp = block[6];
            block[6] = OmgHaxHex.TableS3[0xe00 + (block[14] & 0xff)];
            block[14] = OmgHaxHex.TableS3[0x600 + (tmp & 0xff)];

            tmp = block[3];
            block[3] = OmgHaxHex.TableS3[0xf00 + (block[7] & 0xff)];
            block[7] = OmgHaxHex.TableS3[0x300 + (block[11] & 0xff)];
            block[11] = OmgHaxHex.TableS3[0x700 + (block[15] & 0xff)];
            block[15] = OmgHaxHex.TableS3[0xb00 + (tmp & 0xff)];
        }

        private byte[] PermuteTable2(int i)
        {
            return Utilities.CopyOfRange(OmgHaxHex.TableS4, ((71 * i) % 144) << 8, OmgHaxHex.TableS4.Length);
        }

        private void PermuteBlock2(byte[] block, int round)
        {
            block[0] = PermuteTable2(round * 16 + 0)[(block[0] & 0xff)];
            block[4] = PermuteTable2(round * 16 + 4)[(block[4] & 0xff)];
            block[8] = PermuteTable2(round * 16 + 8)[(block[8] & 0xff)];
            block[12] = PermuteTable2(round * 16 + 12)[(block[12] & 0xff)];

            byte tmp = block[13];
            block[13] = PermuteTable2(round * 16 + 13)[(block[9] & 0xff)];
            block[9] = PermuteTable2(round * 16 + 9)[(block[5] & 0xff)];
            block[5] = PermuteTable2(round * 16 + 5)[(block[1] & 0xff)];
            block[1] = PermuteTable2(round * 16 + 1)[(tmp & 0xff)];

            tmp = block[2];
            block[2] = PermuteTable2(round * 16 + 2)[(block[10] & 0xff)];
            block[10] = PermuteTable2(round * 16 + 10)[(tmp & 0xff)];
            tmp = block[6];
            block[6] = PermuteTable2(round * 16 + 6)[(block[14] & 0xff)];
            block[14] = PermuteTable2(round * 16 + 14)[(tmp & 0xff)];

            tmp = block[3];
            block[3] = PermuteTable2(round * 16 + 3)[(block[7] & 0xff)];
            block[7] = PermuteTable2(round * 16 + 7)[(block[11] & 0xff)];
            block[11] = PermuteTable2(round * 16 + 11)[(block[15] & 0xff)];
            block[15] = PermuteTable2(round * 16 + 15)[(tmp & 0xff)];
        }
    }
}
