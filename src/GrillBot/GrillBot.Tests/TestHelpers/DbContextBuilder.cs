using GrillBot.Database.Services;
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
}
