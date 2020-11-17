using System;
namespace AirPlay.Models
{
    public struct H264Codec
    {
        public byte Compatibility;
        public short LengthOfPps;
        public short LengthOfSps;
        public byte Level;
        public short NumberOfPps;
        public byte[] PictureParameterSet;
        public byte ProfileHigh;
        public byte Reserved3AndSps;
        public byte Reserved6AndNal;
        public byte[] SequenceParameterSet;
        public byte Version;
    }
}
