using System;
using AirPlay.Models;

namespace AirPlay
{
    public interface IRtspReceiver
    {
        void OnSetVolume(decimal volume);
        void OnData(H264Data data);
        void OnPCMData(PcmData data);
    }
}
