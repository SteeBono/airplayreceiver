using AirPlay.Models.Configs;
using AirPlay.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AirPlay
{
    public class Program
    {
        public static IConfigurationRoot Configuration;

        public static async Task Main(string[] args)
        {
            // DUMP WAV AUDIO
            //var dir = Directory.GetFiles("dump/pcm");
            //var _audiobuf = dir.SelectMany(d => File.ReadAllBytes(d)).ToList();
            //using (var wr = new FileStream("dump/dequeued.wav", FileMode.Create))
            //{
            //    var header = Utilities.WriteWavHeader(2, 44100, 16, (uint)_audiobuf.Count);
            //    wr.Write(header, 0, header.Length);
            //}

            //using (FileStream _writer = new FileStream("dump/dequeued.wav", FileMode.Append))
            //{
            //    _writer.Write(_audiobuf.ToArray(), 0, _audiobuf.Count);
            //}

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());

                    var os = "win";
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        os = "osx";
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        os = "linux";
                    }

                    config.AddJsonFile($"appsettings_{os}.json", optional: false, reloadOnChange: false);
                    config.AddEnvironmentVariables();
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();

                    services.Configure<AirPlayReceiverConfig>(hostContext.Configuration.GetSection("AirPlayReceiver"));
                    services.Configure<CodecLibrariesConfig>(hostContext.Configuration.GetSection("CodecLibraries"));
                    services.Configure<DumpConfig>(hostContext.Configuration.GetSection("Dump"));

                    services.AddSingleton<IAirPlayReceiver, AirPlayReceiver>();

                    services.AddHostedService<AirPlayService>();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }
    }
}
