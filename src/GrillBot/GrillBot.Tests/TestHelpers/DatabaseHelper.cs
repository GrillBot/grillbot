using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DatabaseHelper
{
    public static DatabaseContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder()
            .EnableDetailedErrors()
            .EnableThreadSafetyChecks()
            .EnableSensitiveDataLogging()
            .UseInMemoryDatabase("GrillBot");

        return new DatabaseContext(builder.Options);
    }
}
