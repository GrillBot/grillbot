using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Services;
using GrillBot.Database.Services.Repository;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public class DbContextBuilder : GrillBotDatabaseBuilder
{
    public DbContextBuilder() : base(null)
    {
    }

    public override GrillBotContext Create()
    {
        return DatabaseHelper.CreateDbContext();
    }

    public override GrillBotRepository CreateRepository()
    {
        return new GrillBotRepository(Create(), new CounterManager());
    }
}
