using System;
namespace AirPlay.Models.Enums
{
    public enum AudioFormat
    {
        Unknown = -1,
        PCM = 0,
        ALAC = 0x40000,
        AAC = 0x400000,
        AAC_ELD = 0x1000000
    }
}
