using System;
using System.IO;
using AirPlay.Utils;

namespace AirPlay
{
    public class ModifiedMD5
    {
        private int[] _shift = new int[] { 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 7, 12, 17, 22, 5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14, 20, 5, 9, 14, 20, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 4, 11, 16, 23, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21, 6, 10, 15, 21 };

        public void ModifiedMd5(byte[] originalblockIn, byte[] keyIn, byte[] keyOut)
        {
            var blockIn = new byte[64];
            long A, B, C, D, Z, tmp;

            Array.Copy(originalblockIn, 0, blockIn, 0, 64);

            using (var keyInMem = new MemoryStream(keyIn))
            using (var reader = new BinaryReader(keyInMem))
            {
                A = reader.ReadInt32() & 0xffffffffL;
                B = reader.ReadInt32() & 0xffffffffL;
                C = reader.ReadInt32() & 0xffffffffL;
                D = reader.ReadInt32() & 0xffffffffL;

                for (var i = 0; i < 64; i++)
                {
                    int input;
                    var j = 0;
                    if (i < 16)
                    {
                        j = i;
                    }
                    else if (i < 32)
                    {
                        j = (5 * i + 1) % 16;
                    }
                    else if (i < 48)
                    {
                        j = (3 * i + 5) % 16;
                    }
                    else if (i < 64)
                    {
                        j = 7 * i % 16;
                    }

                    input = ((blockIn[4 * j] & 0xFF) << 24) | ((blockIn[4 * j + 1] & 0xFF) << 16) | ((blockIn[4 * j + 2] & 0xFF) << 8) | (blockIn[4 * j + 3] & 0xFF);
                    Z = A + input + (long)((1L << 32) * Math.Abs(Math.Sin(i + 1)));
                    if (i < 16)
                    {
                        Z = Rol(Z + F(B, C, D), _shift[i]);
                    }
                    else if (i < 32)
                    {
                        Z = Rol(Z + G(B, C, D), _shift[i]);
                    }
                    else if (i < 48)
                    {
                        Z = Rol(Z + H(B, C, D), _shift[i]);
                    }
                    else if (i < 64)
                    {
                        Z = Rol(Z + I(B, C, D), _shift[i]);
                    }
                    Z = Z + B;
                    tmp = D;
                    D = C;
                    C = B;
                    B = Z;
                    A = tmp;
                    if (i == 31)
                    {
                        Utilities.Swap(blockIn, 4 * (int)(A & 15), 4 * (int)(B & 15));
                        Utilities.Swap(blockIn, 4 * (int)(C & 15), 4 * (int)(D & 15));
                        Utilities.Swap(blockIn, 4 * (int)((A & (15 << 4)) >> 4), 4 * (int)((B & (15 << 4)) >> 4));
                        Utilities.Swap(blockIn, 4 * (int)((A & (15 << 8)) >> 8), 4 * (int)((B & (15 << 8)) >> 8));
                        Utilities.Swap(blockIn, 4 * (int)((A & (15 << 12)) >> 12), 4 * (int)((B & (15 << 12)) >> 12));
                    }
                }

                using (var keyOutMem = new MemoryStream(keyOut))
                using (var writer = new BinaryWriter(keyOutMem))
                {
                    keyInMem.Position = 0;
                    writer.Write((int)(reader.ReadInt32() + A));

                    keyInMem.Position = 4;
                    writer.Write((int)(reader.ReadInt32() + B));

                    keyInMem.Position = 8;
                    writer.Write((int)(reader.ReadInt32() + C));

                    keyInMem.Position = 12;
                    writer.Write((int)(reader.ReadInt32() + D));
                }
            }
        }

        private long F(long B, long C, long D)
        {
            return (B & C) | (~B & D);
        }

        private long G(long B, long C, long D)
        {
            return (B & D) | (C & ~D);
        }

        private long H(long B, long C, long D)
        {
            return B ^ C ^ D;
        }
        
        private long I(long B, long C, long D)
        {
            return C ^ (B | ~D);
        }

        private long Rol(long input, int count)
        {
            return ((input << count) & 0xffffffffL) | (input & 0xffffffffL) >> (32 - count);
        }
    }
}
