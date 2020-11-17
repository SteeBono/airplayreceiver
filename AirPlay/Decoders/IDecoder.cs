using System;
using System.Threading.Tasks;
using AirPlay.Models.Enums;

namespace AirPlay
{
    public interface IDecoder
    {
        AudioFormat Type { get; }
        int GetOutputStreamLength();
        int Config(int sampleRate, int channels, int bitDepth, int frameLength);
        int DecodeFrame(byte[] input, ref byte[] output, int length);
    }
}
