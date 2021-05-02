using AirPlay.Models.Configs;
using AirPlay.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Plists;
using System.Text;
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

                        return new AirPlayReceiver(config.Instance, codecConfig, config.AirTunesPort, config.AirPlayPort);
                    });
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
