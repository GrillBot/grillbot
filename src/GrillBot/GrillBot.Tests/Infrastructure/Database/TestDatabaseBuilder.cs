using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GrillBot.Database.Services;
using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Tests.Infrastructure.Database;

[ExcludeFromCodeCoverage]
public class TestDatabaseBuilder : GrillBotDatabaseBuilder
{
    private GrillBotContext Context { get; set; }

    public TestDatabaseBuilder() : base(null!)
    {
    }

    private static DatabaseContext CreateContext()
    {
        var builder = new DbContextOptionsBuilder()
            .EnableDetailedErrors()
            .EnableThreadSafetyChecks()
            .EnableSensitiveDataLogging()
            .UseInMemoryDatabase("GrillBot");

        return new DatabaseContext(builder.Options);
    }

    public override GrillBotRepository CreateRepository()
    {
        Context = CreateContext();
        return new GrillBotRepository(Context, TestServices.CounterManager.Value);
    }

    public static void ClearDatabase()
    {
        var context = CreateContext();
        ClearDatabase(context);
    }

    private static void ClearDatabase(GrillBotContext context)
    {
        context.ChangeTracker.Clear();
        context.RemoveRange(context.Users.AsEnumerable());
        context.RemoveRange(context.Guilds.AsEnumerable());
        context.RemoveRange(context.GuildUsers.AsEnumerable());
        context.RemoveRange(context.Channels.AsEnumerable());
        context.RemoveRange(context.UserChannels.AsEnumerable());
        context.RemoveRange(context.Invites.AsEnumerable());
        context.RemoveRange(context.SearchItems.AsEnumerable());
        context.RemoveRange(context.Unverifies.AsEnumerable());
        context.RemoveRange(context.UnverifyLogs.AsEnumerable());
        context.RemoveRange(context.AuditLogs.AsEnumerable());
        context.RemoveRange(context.AuditLogFiles.AsEnumerable());
        context.RemoveRange(context.Emotes.AsEnumerable());
        context.RemoveRange(context.Reminders.AsEnumerable());
        context.RemoveRange(context.SelfunverifyKeepables.AsEnumerable());
        context.RemoveRange(context.ExplicitPermissions.AsEnumerable());
        context.RemoveRange(context.AutoReplies.AsEnumerable());
        context.RemoveRange(context.EmoteSuggestions.AsEnumerable());
        context.RemoveRange(context.GuildEvents.AsEnumerable());
        context.RemoveRange(context.PointsTransactionSummaries.AsEnumerable());
        context.RemoveRange(context.PointsTransactions.AsEnumerable());
        context.SaveChanges();
    }
}
