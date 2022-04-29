using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Discord;

[Initializable]
public class DiscordSyncService : ServiceBase
{
    private ChannelSynchronization Channels { get; }
    private UserSynchronization Users { get; }
    private GuildSynchronization Guilds { get; }
    private GuildUserSynchronization GuildUsers { get; }

    public DiscordSyncService(DiscordSocketClient client, GrillBotContextFactory dbFactory, DiscordInitializationService initializationService)
        : base(client, dbFactory, initializationService)
    {
        Channels = new ChannelSynchronization(DbFactory);
        Users = new UserSynchronization(DbFactory);
        Guilds = new GuildSynchronization(DbFactory);
        GuildUsers = new GuildUserSynchronization(DbFactory);

        DiscordClient.Ready += OnReadyAsync;
        DiscordClient.UserJoined += user => RunAsync(() => GuildUsers.UserJoinedAsync(user));

        DiscordClient.GuildMemberUpdated += (before, after) => RunAsync(
            () => GuildUsers.GuildMemberUpdatedAsync(before.Value, after),
            () => before.HasValue && (before.Value.Nickname != after.Nickname || before.Value.Username != after.Username || before.Value.Discriminator != after.Discriminator)
        );

        DiscordClient.JoinedGuild += guild => RunAsync(() => Guilds.GuildAvailableAsync(guild));
        DiscordClient.GuildAvailable += guild => RunAsync(() => Guilds.GuildAvailableAsync(guild));

        DiscordClient.GuildUpdated += (before, after) => RunAsync(
            () => Guilds.GuildUpdatedAsync(before, after),
            () => before.Name != after.Name || !before.Roles.SequenceEqual(after.Roles)
        );

        DiscordClient.UserUpdated += (before, after) => RunAsync(
            () => Users.UserUpdatedAsync(before, after),
            () => before.Username != after.Username || before.Discriminator != after.Discriminator || before.IsUser() != after.IsUser()
        );

        DiscordClient.ChannelUpdated += (before, after) => RunAsync(
            () => Channels.ChannelUpdatedAsync(before as ITextChannel, after as ITextChannel),
            () => before is ITextChannel && after is ITextChannel
        );

        DiscordClient.ThreadDeleted += thread => RunAsync(
            () => Channels.ThreadDeletedAsync(thread.Value),
            () => thread.HasValue
        );

        DiscordClient.ChannelDestroyed += channel => RunAsync(
            () => Channels.ChannelDeletedAsync(channel as ITextChannel),
            () => channel is ITextChannel
        );

        DiscordClient.ThreadUpdated += (before, after) => RunAsync(
            () => Channels.ThreadUpdatedAsync(before.Value, after),
            () => before.HasValue
        );
    }

    private async Task RunAsync(Func<Task> syncFunction, Func<bool> check = null)
    {
        if (!InitializationService.Get()) return;
        if (check != null && !check()) return;
        if (await CheckPendingMigrationsAsync()) return;

        await syncFunction();
    }

    private async Task OnReadyAsync()
    {
        using var context = DbFactory.Create();

        var dbChannels = await context.Channels.ToListAsync();
        dbChannels.ForEach(o => o.Flags |= (long)ChannelFlags.Deleted);

        foreach (var guild in DiscordClient.Guilds)
        {
            await GuildUsers.InitUsersAsync(context, guild);
            await Channels.InitChannelsAsync(guild, dbChannels);
        }

        await Users.InitBotAdminAsync(context, await DiscordClient.GetApplicationInfoAsync());
        await context.SaveChangesAsync();
    }
}
