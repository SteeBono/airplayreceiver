using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AirPlay.Listeners;
using AirPlay.Models;
using Makaretu.Dns;

namespace AirPlay
{
    public class AirPlayReceiver : IRtspReceiver
    {
        private MulticastService _mdns = null;
        private AirTunesListener _airTunesListener = null;
        private string _instance = string.Empty;
        private ushort _airTunesPort;
        private ushort _airPlayPort;

        public const string AirPlayType = "_airplay._tcp";
        public const string AirTunesType = "_raop._tcp";

        public event EventHandler<decimal> OnSetVolumeReceived;
        public event EventHandler<H264Data> OnH264DataReceived;
        public event EventHandler<PcmData> OnPCMDataReceived;

        public AirPlayReceiver(string instance, ushort airTunesPort = 5000, ushort airPlayPort = 7000)
        {
            _instance = instance;
            _airTunesPort = airTunesPort;
            _airPlayPort = airPlayPort;

            _airTunesListener = new AirTunesListener(this, _airTunesPort, _airPlayPort);
        }

        public async Task StartListeners(CancellationToken cancellationToken)
        {
            await _airTunesListener.StartAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StartMdnsAsync(string deviceId)
        {
            if(string.IsNullOrWhiteSpace(deviceId))
            {
                throw new ArgumentNullException(deviceId);
            }

            var rDeviceId = new Regex("^(([0-9a-fA-F][0-9a-fA-F]):){5}([0-9a-fA-F][0-9a-fA-F])$");
            var mDeviceId = rDeviceId.Match(deviceId);
            if (!mDeviceId.Success)
            {
                throw new ArgumentException("Device id must be a mac address", deviceId);
            }

            var deviceIdInstance = string.Join(string.Empty, mDeviceId.Groups[2].Captures) + mDeviceId.Groups[3].Value;

            _mdns = new MulticastService();
            var sd = new ServiceDiscovery(_mdns);

#if DEBUG
            foreach (var a in MulticastService.GetIPAddresses())
            {
                Console.WriteLine($"IP address {a}");
            }
            _mdns.NetworkInterfaceDiscovered += (s, e) =>
            {
                foreach (var nic in e.NetworkInterfaces)
                {
                    Console.WriteLine($"NIC '{nic.Name}'");
                }
            };
#endif

            // Internally 'ServiceProfile' create the SRV record
            var airTunes = new ServiceProfile($"{deviceIdInstance}@{_instance}", AirTunesType, _airTunesPort);
            airTunes.AddProperty("ch", "2");
            airTunes.AddProperty("cn", "2,3");
            airTunes.AddProperty("et", "0,3,5");
            airTunes.AddProperty("md", "0,1,2");
            airTunes.AddProperty("sr", "44100");
            airTunes.AddProperty("ss", "16");
            airTunes.AddProperty("da", "true");
            airTunes.AddProperty("sv", "false");
            airTunes.AddProperty("ft", "0x5A7FFFF7,0x1E"); // 0x4A7FFFF7, 0xE
            airTunes.AddProperty("am", "AppleTV5,3");
            airTunes.AddProperty("pk", "29fbb183a58b466e05b9ab667b3c429d18a6b785637333d3f0f3a34baa89f45e");
            airTunes.AddProperty("sf", "0x4");
            airTunes.AddProperty("tp", "UDP");
            airTunes.AddProperty("vn", "65537");
            airTunes.AddProperty("vs", "220.68");
            airTunes.AddProperty("vv", "2");

            /*
             * ch	2	audio channels: stereo
             * cn	0,1,2,3	audio codecs
             * et	0,3,5	supported encryption types
             * md	0,1,2	supported metadata types
             * pw	false	does the speaker require a password?
             * sr	44100	audio sample rate: 44100 Hz
             * ss	16	audio sample size: 16-bit
             */

            // Internally 'ServiceProfile' create the SRV record
            var airPlay = new ServiceProfile(_instance, AirPlayType, _airPlayPort);
            airPlay.AddProperty("deviceid", deviceId);
            airPlay.AddProperty("features", "0x5A7FFFF7,0x1E"); // 0x4A7FFFF7
            airPlay.AddProperty("flags", "0x4");
            airPlay.AddProperty("model", "AppleTV5,3");
            airPlay.AddProperty("pk", "29fbb183a58b466e05b9ab667b3c429d18a6b785637333d3f0f3a34baa89f45e");
            airPlay.AddProperty("pi", "aa072a95-0318-4ec3-b042-4992495877d3");
            airPlay.AddProperty("srcvers", "220.68");
            airPlay.AddProperty("vv", "2");

            sd.Advertise(airTunes);
            sd.Advertise(airPlay);

            _mdns.Start();

            return Task.CompletedTask;
        }

        public void OnSetVolume(decimal volume)
        {
            OnSetVolumeReceived?.Invoke(this, volume);
        }

        public void OnData(H264Data data)
        {
            OnH264DataReceived?.Invoke(this, data);
        }

        public void OnPCMData(PcmData data)
        {
            OnPCMDataReceived?.Invoke(this, data);
        }
    }
}