using GrillBot.Database.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public class DbContextBuilder : GrillBotContextFactory
{
    public DbContextBuilder() : base(null)
    {
    }

    public override GrillBotContext Create()
    {
        return DatabaseHelper.CreateDbContext();
    }
}
