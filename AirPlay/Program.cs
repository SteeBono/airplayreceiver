using AirPlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Plists;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirPlay
{
    public class Program
    {
        private static CancellationTokenSource _cancellationTokenSource;

        public static async Task Main(string[] args)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if(Directory.Exists("/Users/steebono/desktop/dump/frames"))
                Directory.Delete("/Users/steebono/desktop/dump/frames", true);
            if (Directory.Exists("/Users/steebono/desktop/dump/pcm"))
                Directory.Delete("/Users/steebono/desktop/dump/pcm", true);
            if (Directory.Exists("/Users/steebono/desktop/dump/out"))
                Directory.Delete("/Users/steebono/desktop/dump/out", true);

            Directory.CreateDirectory("/Users/steebono/desktop/dump/frames");
            Directory.CreateDirectory("/Users/steebono/desktop/dump/pcm");
            Directory.CreateDirectory("/Users/steebono/desktop/dump/out");

            var deviceId = "11:22:33:44:55:66";

            var receiver = new AirPlayReceiver("steebono-test-1", 5000, 7000);

            await receiver.StartListeners(_cancellationTokenSource.Token);
            await receiver.StartMdnsAsync(deviceId).ConfigureAwait(false);

            receiver.OnSetVolumeReceived += (s, e) =>
            {

            };

            receiver.OnH264DataReceived += (s, e) =>
            {
                // DO SOMETHING WITH VIDEO DATA..
                using (FileStream _writer = new FileStream("/Users/steebono/Desktop/dump/dump.h264", FileMode.Append))
                {
                    _writer.Write(e.Data, 0, e.Length);
                }
            };

            var audiobuf = new List<byte>();
            receiver.OnPCMDataReceived += (s, e) =>
            {
                // DO SOMETHING WITH AUDIO DATA..
                audiobuf.AddRange(e.Data);
            };

            Console.ReadKey();

            using (var wr = new FileStream("/Users/steebono/Desktop/dump/dequeued.wav", FileMode.Create))
            {
                var header = Utilities.WriteWavHeader(2, 44100, 16, (uint)audiobuf.Count);
                wr.Write(header, 0, header.Length);
            }

            using (FileStream _writer = new FileStream("/Users/steebono/Desktop/dump/dequeued.wav", FileMode.Append))
            {
                _writer.Write(audiobuf.ToArray(), 0, audiobuf.Count);
            }
        }
    }
}
