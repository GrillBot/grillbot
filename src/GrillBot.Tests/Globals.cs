global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using GrillBot.Tests.TestHelpers;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using GrillBot.Tests.Common;
global using GrillBot.Tests.Infrastructure;
global using System;
global using GrillBot.Tests.Infrastructure.Database;
global using GrillBot.Common.Models;
global using GrillBot.Tests.Infrastructure.Cache;
global using Microsoft.Extensions.Configuration;
using AutoMapper;
using Discord.WebSocket;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.Graphics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GrillBot.Tests;

public static class TestServices
{
    public static readonly Lazy<TestDatabaseBuilder> DatabaseBuilder = new();
    public static readonly Lazy<TestCacheBuilder> CacheBuilder = new();
    public static readonly Lazy<IConfiguration> Configuration = new(() => ConfigurationHelper.CreateConfiguration());
    public static readonly Lazy<IMapper> AutoMapper = new(AutoMapperHelper.CreateMapper);
    public static readonly Lazy<CounterManager> CounterManager = new();
    public static readonly Lazy<IServiceProvider> Provider = new(DiHelper.CreateProvider, LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<ILoggerFactory> LoggerFactory = new(() => NullLoggerFactory.Instance, LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<RandomizationManager> Random = new(() => new RandomizationManager(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<IWebHostEnvironment> TestingEnvironment = new(() => new EnvironmentBuilder().AsTest().Build(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<ITextsManager> Texts = new(() => new TextsManager("./Resources", "messages"), LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<IGraphicsClient> Graphics = new(() => new GraphicsClientBuilder().SetAll().Build(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<DiscordSocketClient> DiscordSocketClient = new(() => new DiscordSocketClient(), LazyThreadSafetyMode.ExecutionAndPublication);
}
