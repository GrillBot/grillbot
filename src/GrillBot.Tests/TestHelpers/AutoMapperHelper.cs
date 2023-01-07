using AutoMapper;
using GrillBot.App;
using GrillBot.Database.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using GrillBot.Cache.Services;
using GrillBot.Common;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class AutoMapperHelper
{
    public static IMapper CreateMapper()
    {
        var botAssembly = typeof(Startup).Assembly;

        var profiles = botAssembly
            .GetReferencedAssemblies()
            .Where(o => !string.IsNullOrEmpty(o.Name) && o.Name.StartsWith("GrillBot"))
            .SelectMany(o => Assembly.Load(o).GetTypes())
            .Union(botAssembly.GetTypes())
            .Distinct()
            .Where(o => !o.IsAbstract && typeof(Profile).IsAssignableFrom(o))
            .Select(o => Activator.CreateInstance(o) as Profile)
            .Where(o => o != null)
            .ToList();

        var conf = new MapperConfiguration(c => c.AddProfiles(profiles));
        return conf.CreateMapper();
    }
}
