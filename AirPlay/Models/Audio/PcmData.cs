using System;
namespace AirPlay.Models
{
    public struct PcmData
    {
        public int Length { get; set; }
        public byte[] Data { get; set; }
        public ulong Pts { get; set; }
    }
}
