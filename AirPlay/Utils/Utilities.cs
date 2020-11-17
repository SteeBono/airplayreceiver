using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AirPlay.Utils
{
    public static class Utilities
    {
        public static byte[] CopyOfRange(byte[] src, int start, int end)
        {
            int len = end - start;
            byte[] dest = new byte[len];
            Array.Copy(src, start, dest, 0, len);
            return dest;
        }

        public static byte[] Hash(byte[] first, byte[] last)
        {
            var sha512 = new SHA512CryptoServiceProvider();
            byte[] combined = first.Concat(last).ToArray();
            byte[] hashed = sha512.ComputeHash(combined);
            return hashed;
        }

        public static ushort SeqNumCmp(int s1, int s2)
        {
            return (ushort)(s1 - s2);
        }

        public static void Swap(byte[] arr, int idxA, int idxB)
        {
            using (var mem = new MemoryStream(arr))
            using (var reader = new BinaryReader(mem))
            using (var writer = new BinaryWriter(mem))
            {
                mem.Position = idxA;
                var a = reader.ReadInt32();

                mem.Position = idxB;
                var b = reader.ReadInt32();

                mem.Position = idxB;
                writer.Write(a);

                mem.Position = idxA;
                writer.Write(b);
            }
        }

        public static byte[] WriteWavHeader(ushort numchannels, uint sampleRate, ushort bitsPerSample, uint tot)
        {
            var stream = new MemoryStream();

            BinaryWriter bwl = new BinaryWriter(stream);
            bwl.Write(new char[4] { 'R', 'I', 'F', 'F' });
            bwl.Write(tot + 38);
            bwl.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bwl.Write((int)bitsPerSample);
            bwl.Write((short)1);
            bwl.Write(numchannels);
            bwl.Write(sampleRate);
            bwl.Write((int)(sampleRate * ((bitsPerSample * numchannels) / 8)));
            bwl.Write((short)((bitsPerSample * numchannels) / 8));
            bwl.Write(bitsPerSample);
            bwl.Write(new char[4] { 'd', 'a', 't', 'a' });
            bwl.Write(tot);

            return stream.ToArray();
        }
    }
}
