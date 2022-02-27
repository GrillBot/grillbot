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
global using GrillBot.Data;
global using GrillBot.Database;
global using GrillBot.Database.Services;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel;
global using Microsoft.Extensions.Configuration;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;
global using System.Globalization;
global using System.Text;
global using Humanizer;
global using Humanizer.Localisation;
global using Microsoft.EntityFrameworkCore;
global using System.Collections.Concurrent;
global using GrillBot.App.Extensions;
global using GrillBot.App.Extensions.Discord;
global using GrillBot.Data.Extensions.Discord;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App
{
    static public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}
