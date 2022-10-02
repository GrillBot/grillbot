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
using GrillBot.App.Services;
using GrillBot.Common.Managers.Counters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace GrillBot.Tests;

public static class TestServices
{
    public static readonly Lazy<TestDatabaseBuilder> DatabaseBuilder = new();
    public static readonly Lazy<TestCacheBuilder> CacheBuilder = new();
    public static readonly Lazy<IConfiguration> Configuration = new(() => ConfigurationHelper.CreateConfiguration());
    public static readonly Lazy<IMapper> AutoMapper = new(AutoMapperHelper.CreateMapper);
    public static readonly Lazy<CounterManager> CounterManager = new();
    public static readonly Lazy<IServiceProvider> EmptyProvider = new(DiHelper.CreateEmptyProvider, LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<IServiceProvider> InitializedProvider = new(DiHelper.CreateInitializedProvider, LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<ILoggerFactory> LoggerFactory = new(LoggingHelper.CreateLoggerFactory, LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<RandomizationService> Randomization = new(() => new RandomizationService(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<IWebHostEnvironment> TestingEnvironment = new(() => new EnvironmentBuilder().AsTest().Build(), LazyThreadSafetyMode.ExecutionAndPublication);
    public static readonly Lazy<IWebHostEnvironment> ProductionEnvironment = new(() => new EnvironmentBuilder().AsProd().Build(), LazyThreadSafetyMode.ExecutionAndPublication);
}
