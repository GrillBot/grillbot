global using System;
global using System.Net;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.IO;
global using Discord;
global using Discord.Rest;
global using Discord.WebSocket;
global using GrillBot.Database;
global using GrillBot.Database.Services;
global using System.ComponentModel.DataAnnotations;
global using Microsoft.Extensions.Configuration;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;
global using System.Globalization;
global using System.Text;
global using Humanizer;
global using GrillBot.App.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;

namespace GrillBot.App;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            )
            .ConfigureLogging(builder =>
            {
                builder.AddOpenTelemetry(opt =>
                {
                    opt.IncludeFormattedMessage = true;
                    opt.IncludeScopes = true;

                    var resourceBuilder = ResourceBuilder.CreateDefault();
                    resourceBuilder.AddService("GrillBot");
                    opt.SetResourceBuilder(resourceBuilder);

                    opt.AddOtlpExporter();
                });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureKestrel(opt => opt.AddServerHeader = false);
            });
}
