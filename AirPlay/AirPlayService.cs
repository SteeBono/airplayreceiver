using AirPlay.Models.Configs;
using AirPlay.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AirPlay
{
    public class AirPlayService : IHostedService, IDisposable
    {
        private readonly IAirPlayReceiver _airPlayReceiver;
        private readonly DumpConfig _dConfig;

        private List<byte> _audiobuf;

        public AirPlayService(IAirPlayReceiver airPlayReceiver, IOptions<DumpConfig> dConfig)
        {
            _airPlayReceiver = airPlayReceiver ?? throw new ArgumentNullException(nameof(airPlayReceiver));
            _dConfig = dConfig?.Value ?? throw new ArgumentNullException(nameof(dConfig));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
#if DUMP
            var bPath = _dConfig.Path;
            var fPath = Path.Combine(bPath, "frames/");
            var oPath = Path.Combine(bPath, "out/");
            var pPath = Path.Combine(bPath, "pcm/");

            if (!Directory.Exists(bPath))
            {
                Directory.CreateDirectory(bPath);
            }
            if (!Directory.Exists(fPath))
            {
                Directory.CreateDirectory(fPath);
            }
            if (!Directory.Exists(oPath))
            {
                Directory.CreateDirectory(oPath);
            }
            if (!Directory.Exists(pPath))
            {
                Directory.CreateDirectory(pPath);
            }
#endif

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
                using (FileStream writer = new FileStream($"{bPath}dump.h264", FileMode.Append))
                {
                    writer.Write(e.Data, 0, e.Length);
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
            var bPath = _dConfig.Path;
            using (var wr = new FileStream($"{bPath}dequeued.wav", FileMode.Create))
            {
                var header = Utilities.WriteWavHeader(2, 44100, 16, (uint)_audiobuf.Count);
                wr.Write(header, 0, header.Length);
            }

            using (FileStream writer = new FileStream($"{bPath}dequeued.wav", FileMode.Append))
            {
                writer.Write(_audiobuf.ToArray(), 0, _audiobuf.Count);
            }
#endif
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
