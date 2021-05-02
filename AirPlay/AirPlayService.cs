using AirPlay.Utils;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AirPlay
{
    public class AirPlayService : IHostedService, IDisposable
    {
        private readonly AirPlayReceiver _airPlayReceiver;

        private List<byte> _audiobuf;

        public AirPlayService(AirPlayReceiver airPlayReceiver)
        {
            _airPlayReceiver = airPlayReceiver ?? throw new ArgumentNullException(nameof(airPlayReceiver));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _airPlayReceiver.StartListeners(cancellationToken);
            await _airPlayReceiver.StartMdnsAsync().ConfigureAwait(false);

            _airPlayReceiver.OnSetVolumeReceived += (s, e) =>
            {
                // SET VOLUME
            };

            // DUMP H264 VIDEO
            _airPlayReceiver.OnH264DataReceived += (s, e) =>
            {
                // DO SOMETHING WITH VIDEO DATA..
#if DUMP
                using (FileStream _writer = new FileStream("/Users/steebono/Desktop/dump/dump.h264", FileMode.Append))
                {
                    _writer.Write(e.Data, 0, e.Length);
                }
#endif
            };

            _audiobuf = new List<byte>();
            _airPlayReceiver.OnPCMDataReceived += (s, e) =>
            {
                // DO SOMETHING WITH AUDIO DATA..
#if DUMP
                _audiobuf.AddRange(e.Data);
#endif
            };
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
#if DUMP
            // DUMP WAV AUDIO
            using (var wr = new FileStream("/Users/steebono/Desktop/dump/dequeued.wav", FileMode.Create))
            {
                var header = Utilities.WriteWavHeader(2, 44100, 16, (uint)_audiobuf.Count);
                wr.Write(header, 0, header.Length);
            }

            using (FileStream _writer = new FileStream("/Users/steebono/Desktop/dump/dequeued.wav", FileMode.Append))
            {
                _writer.Write(_audiobuf.ToArray(), 0, _audiobuf.Count);
            }
#endif
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
