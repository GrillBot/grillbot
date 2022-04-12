using AutoMapper;
using GrillBot.App;
using GrillBot.Data;
using GrillBot.Database.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class AutoMapperHelper
{
    public static IMapper CreateMapper()
    {
        var profiles = new[]
        {
            typeof(Startup).Assembly.GetTypes(),
            typeof(Emojis).Assembly.GetTypes(),
            typeof(GrillBotContext).Assembly.GetTypes()
        }
        .SelectMany(o => o)
        .Where(o => !o.IsAbstract && typeof(Profile).IsAssignableFrom(o))
        .Select(o => Activator.CreateInstance(o) as Profile)
        .Where(o => o != null)
        .ToList();

        var conf = new MapperConfiguration(c => c.AddProfiles(profiles));
        return conf.CreateMapper();
    }
}
