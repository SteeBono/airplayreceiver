using AirPlay.Models.Configs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirPlay
{
    public class AirPlayService : IHostedService, IDisposable
    {
        private readonly AirPlayReceiver _airPlayReceiver;

        public AirPlayService(AirPlayReceiver airPlayReceiver)
        {
            _airPlayReceiver = airPlayReceiver ?? throw new ArgumentNullException(nameof(airPlayReceiver));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var deviceId = "11:22:33:44:55:66";

            await _airPlayReceiver.StartListeners(cancellationToken);
            await _airPlayReceiver.StartMdnsAsync(deviceId).ConfigureAwait(false);

            _airPlayReceiver.OnSetVolumeReceived += (s, e) =>
            {
                // SET VOLUME
            };

            // DUMP H264 VIDEO
            _airPlayReceiver.OnH264DataReceived += (s, e) =>
            {
                // DO SOMETHING WITH VIDEO DATA..
                //using (FileStream _writer = new FileStream("/Users/steebono/Desktop/dump/dump.h264", FileMode.Append))
                //{
                //    _writer.Write(e.Data, 0, e.Length);
                //}
            };

            var audiobuf = new List<byte>();
            _airPlayReceiver.OnPCMDataReceived += (s, e) =>
            {
                // DO SOMETHING WITH AUDIO DATA..
                audiobuf.AddRange(e.Data);
            };

            Console.ReadKey();

            // DUMP WAV AUDIO
            //using (var wr = new FileStream("/Users/steebono/Desktop/dump/dequeued.wav", FileMode.Create))
            //{
            //    var header = Utilities.WriteWavHeader(2, 44100, 16, (uint)audiobuf.Count);
            //    wr.Write(header, 0, header.Length);
            //}

            //using (FileStream _writer = new FileStream("/Users/steebono/Desktop/dump/dequeued.wav", FileMode.Append))
            //{
            //    _writer.Write(audiobuf.ToArray(), 0, audiobuf.Count);
            //}
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
