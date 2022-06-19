using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Services;
using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Tests.Infrastructure.Database;

[ExcludeFromCodeCoverage]
public class TestDatabaseBuilder : GrillBotDatabaseBuilder
{
    private GrillBotContext Context { get; set; }

    public TestDatabaseBuilder() : base(DiHelper.CreateEmptyProvider())
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
        return new GrillBotRepository(Context, new CounterManager());
    }

    public void ClearDatabase()
    {
        Context.ChangeTracker.Clear();
        Context.RemoveRange(Context.Users.AsEnumerable());
        Context.RemoveRange(Context.Guilds.AsEnumerable());
        Context.RemoveRange(Context.GuildUsers.AsEnumerable());
        Context.RemoveRange(Context.Channels.AsEnumerable());
        Context.RemoveRange(Context.UserChannels.AsEnumerable());
        Context.RemoveRange(Context.Invites.AsEnumerable());
        Context.RemoveRange(Context.SearchItems.AsEnumerable());
        Context.RemoveRange(Context.Unverifies.AsEnumerable());
        Context.RemoveRange(Context.UnverifyLogs.AsEnumerable());
        Context.RemoveRange(Context.AuditLogs.AsEnumerable());
        Context.RemoveRange(Context.AuditLogFiles.AsEnumerable());
        Context.RemoveRange(Context.Emotes.AsEnumerable());
        Context.RemoveRange(Context.Reminders.AsEnumerable());
        Context.RemoveRange(Context.SelfunverifyKeepables.AsEnumerable());
        Context.RemoveRange(Context.ExplicitPermissions.AsEnumerable());
        Context.RemoveRange(Context.AutoReplies.AsEnumerable());
        Context.RemoveRange(Context.Suggestions.AsEnumerable());
        Context.SaveChanges();
    }
}
