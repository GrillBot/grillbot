using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

    public static void ClearDatabase(GrillBotContext context)
    {
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
        context.RemoveRange(context.MessageCacheIndexes.AsEnumerable());
        context.RemoveRange(context.Suggestions.AsEnumerable());
        context.SaveChanges();
    }
}
