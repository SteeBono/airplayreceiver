/*  
 * I have mapped only used methods.
 * This code does not have all 'ALAC Decoder' functionality
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using AirPlay.Models.Enums;
using AirPlay.Utils;

namespace AirPlay
{
    public unsafe class ALACDecoder : IDecoder, IDisposable
    {
        private IntPtr _handle;
        private IntPtr _decoder;

        private delegate IntPtr alacDecoder_InitializeDecoder(int sampleRate, int channels, int bitsPerSample, int framesPerPacket);
        private delegate int alacDecoder_DecodeFrame(IntPtr decoder, IntPtr inBuffer, IntPtr outBuffer, int* ioNumBytes);

        private alacDecoder_InitializeDecoder _alacDecoder_InitializeDecoder;
        private alacDecoder_DecodeFrame _alacDecoder_DecodeFrame;

        private int _pcm_pkt_size = 0;

        public AudioFormat Type => AudioFormat.ALAC;

        public ALACDecoder(string libraryPath)
        {
            if (!File.Exists(libraryPath))
            {
                throw new IOException("Library not found.");
            }

            // Open library
            _handle = LibraryLoader.DlOpen(libraryPath, 0);

            // Get a function pointer symbol
            IntPtr symAlacDecoder_InitializeDecoder = LibraryLoader.DlSym(_handle, "InitializeDecoder");
            IntPtr symAlacDecoder_DecodeFrame = LibraryLoader.DlSym(_handle, "Decode");

            // Get a delegate for the function pointer
            _alacDecoder_InitializeDecoder = Marshal.GetDelegateForFunctionPointer<alacDecoder_InitializeDecoder>(symAlacDecoder_InitializeDecoder);
            _alacDecoder_DecodeFrame = Marshal.GetDelegateForFunctionPointer<alacDecoder_DecodeFrame>(symAlacDecoder_DecodeFrame);
        }

        public int Config(int sampleRate, int channels, int bitDepth, int frameLength)
        {
            _pcm_pkt_size = frameLength * channels * bitDepth / 8;

            _decoder = _alacDecoder_InitializeDecoder(sampleRate, channels, bitDepth, frameLength);

            return _decoder != IntPtr.Zero ? 0 : -1;
        }

        public int GetOutputStreamLength()
        {
            return _pcm_pkt_size;
        }

        public int DecodeFrame(byte[] input, ref byte[] output, int outputLen)
        {
            var size = Marshal.SizeOf(input[0]) * input.Length;
            var inputPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(input, 0, inputPtr, input.Length);

            var outSize = Marshal.SizeOf(output[0]) * output.Length;
            var outPtr = Marshal.AllocHGlobal(outSize);

            var res = _alacDecoder_DecodeFrame(_decoder, inputPtr, outPtr, &outputLen);
            if(res == 0)
            {
                Marshal.Copy(outPtr, output, 0, outputLen);
            }

            return res;
        }

        public void Dispose()
        {
            // Close the C++ library
            LibraryLoader.DlClose(_handle);
            Marshal.FreeBSTR(_handle);
        }
    }

    public struct MagicCookie
    {
        public ALACSpecificConfig config;
        public ALACAudioChannelLayout channelLayoutInfo; // seems to be unused
    }

    public struct ALACSpecificConfig
    {
        public uint frameLength;
        public byte compatibleVersion;
        public byte bitDepth; // max 32
        public byte pb; // 0 <= pb <= 255
        public byte mb;
        public byte kb;
        public byte numChannels;
        public ushort maxRun;
        public uint maxFrameBytes;
        public uint avgBitRate;
        public uint sampleRate;
    }

    public struct ALACAudioChannelLayout
    {
        public uint mChannelLayoutTag;
        public uint mChannelBitmap;
        public uint mNumberChannelDescriptions;
    }
}
