using System;
using AirPlay.Models.Enums;

namespace AirPlay
{
    public class PCMDecoder : IDecoder
    {
        public AudioFormat Type => AudioFormat.PCM;

        public int Config(int sampleRate, int channels, int bitDepth, int frameLength)
        {
            return 0;
        }

        public int DecodeFrame(byte[] input, ref byte[] output, int length)
        {
            Array.Copy(input, 0, output, 0, input.Length);
            return 0;
        }

        public int GetOutputStreamLength()
        {
            return -1;
        }
    }
}
