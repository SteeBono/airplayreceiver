using System;
using System.IO;

namespace AirPlay.Models
{
    public class MirroringHeader
    {
        public int PayloadSize { get; }
        public short PayloadType { get; }
        public short PayloadOption { get; }
        public long PayloadNtp { get; }
        public long PayloadPts { get; }
        public int WidthSource { get; }
        public int HeightSource { get; }
        public int Width { get; }
        public int Height { get; }

        public MirroringHeader(byte[] header)
        {
            var mem = new MemoryStream(header);
            using(var reader = new BinaryReader(mem))
            {
                PayloadSize = (int)reader.ReadUInt32();
                PayloadType = (short)(reader.ReadUInt16() & 0xff);
                PayloadOption = (short)reader.ReadUInt16();

                if(PayloadType == 0)
                {
                    PayloadNtp = (long)reader.ReadUInt64();
                    PayloadPts = NtpToPts(PayloadNtp);
                }
                if (PayloadType == 1)
                {
                    mem.Position = 40;
                    WidthSource = (int)reader.ReadSingle();
                    HeightSource = (int)reader.ReadSingle();

                    mem.Position = 56;
                    Width = (int)reader.ReadSingle();
                    Height = (int)reader.ReadSingle();
                }
            }
        }

        private long NtpToPts(long ntp)
        {
            return (((ntp >> 32) & 0xffffffff) * 1000000) + ((ntp & 0xffffffff) * 1000 * 1000 / Int32.MaxValue);
        }
    }
}
