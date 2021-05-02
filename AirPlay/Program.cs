using AirPlay.Models.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
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

                    services.AddHostedService<AirPlayService>();
                    services.AddSingleton(ctx =>
                    {
                        var config = ctx.GetService<IOptions<AirPlayReceiverConfig>>()?.Value ?? throw new ArgumentNullException("airplayreveicerconfig");
                        var codecConfig = ctx.GetService<IOptions<CodecLibrariesConfig>>()?.Value ?? throw new ArgumentNullException("codeclibrariesconfig");

                        return new AirPlayReceiver(config.Instance, codecConfig, config.AirTunesPort, config.AirPlayPort, config.DeviceMacAddress);
                    });
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

#if DUMP
            // Replace '/Users/steebono/Desktop/dump/' in all source codes w/ your path
            if (!Directory.Exists("/Users/steebono/Desktop/dump/"))
            {
                Directory.CreateDirectory("/Users/steebono/Desktop/dump/");
            }
            if (!Directory.Exists("/Users/steebono/Desktop/dump/frames/"))
            {
                Directory.CreateDirectory("/Users/steebono/Desktop/dump/frames/");
            }
            if (!Directory.Exists("/Users/steebono/Desktop/dump/out/"))
            {
                Directory.CreateDirectory("/Users/steebono/Desktop/dump/out/");
            }
            if (!Directory.Exists("/Users/steebono/Desktop/dump/pcm/"))
            {
                Directory.CreateDirectory("/Users/steebono/Desktop/dump/pcm/");
            }
#endif
            await builder.RunConsoleAsync();
        }
    }
}
