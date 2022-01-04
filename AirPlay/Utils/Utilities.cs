using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace AirPlay.Utils
{
    public static class Utilities
    {
        public const string PAIR_VERIFY_AES_KEY = "Pair-Verify-AES-Key";
        public const string PAIR_VERIFY_AES_IV = "Pair-Verify-AES-IV";

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

        public static IBufferedCipher InitializeChiper(byte[] ecdhShared)
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
    }
}
