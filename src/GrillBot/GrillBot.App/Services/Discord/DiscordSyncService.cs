using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.Discord;

[Initializable]
public class DiscordSyncService
{
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private ChannelSynchronization Channels { get; }
    private UserSynchronization Users { get; }
    private GuildSynchronization Guilds { get; }
    private GuildUserSynchronization GuildUsers { get; }

    private bool MigrationsChecked { get; set; }

    public DiscordSyncService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, InitManager initManager)
    {
        InitManager = initManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        Channels = new ChannelSynchronization(DatabaseBuilder);
        Users = new UserSynchronization(DatabaseBuilder);
        Guilds = new GuildSynchronization(DatabaseBuilder);
        GuildUsers = new GuildUserSynchronization(DatabaseBuilder);

        DiscordClient.Ready += OnReadyAsync;
        DiscordClient.UserJoined += user => RunAsync(() => GuildUsers.UserJoinedAsync(user));

        DiscordClient.GuildMemberUpdated += (before, after) => RunAsync(
            () => GuildUsers.GuildMemberUpdatedAsync(after),
            () => before.HasValue && (before.Value.Nickname != after.Nickname || before.Value.Username != after.Username || before.Value.Discriminator != after.Discriminator)
        );

        DiscordClient.JoinedGuild += guild => RunAsync(() => Guilds.GuildAvailableAsync(guild));
        DiscordClient.GuildAvailable += guild => RunAsync(() => Guilds.GuildAvailableAsync(guild));

        DiscordClient.GuildUpdated += (before, after) => RunAsync(
            () => Guilds.GuildUpdatedAsync(after),
            () => before.Name != after.Name || !before.Roles.SequenceEqual(after.Roles)
        );

        DiscordClient.UserUpdated += (before, after) => RunAsync(
            () => Users.UserUpdatedAsync(after),
            () => before.Username != after.Username || before.Discriminator != after.Discriminator || before.IsUser() != after.IsUser()
        );

        DiscordClient.ChannelUpdated += (_, after) => RunAsync(
            () => Channels.ChannelUpdatedAsync(after as ITextChannel),
            () => after is ITextChannel
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
            () => Channels.ThreadUpdatedAsync(after),
            () => before.HasValue && (before.Value.Name != after.Name || before.Value.IsArchived != after.IsArchived)
        );
    }

    private async Task RunAsync(Func<Task> syncFunction, Func<bool> check = null)
    {
        if (!InitManager.Get()) return;
        if (check != null && !check()) return;

        if (!MigrationsChecked)
        {
            await using var repository = DatabaseBuilder.CreateRepository();
            await repository.ProcessMigrationsAsync();
            MigrationsChecked = true;
        }

        await syncFunction();
    }

    private async Task OnReadyAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await ProcessChannelInitializationAsync(repository);
        await ProcessUsersInitializationAsync(repository);
        await ProcessBotAdminInitialization(repository);
    }

    private async Task ProcessChannelInitializationAsync(GrillBotRepository repository)
    {
        var channels = await repository.Channel.GetAllChannelsAsync();
        foreach (var channel in channels)
        {
            channel.MarkDeleted(true);
            channel.RolePermissionsCount = 0;
            channel.UserPermissionsCount = 0;
        }

        foreach (var guild in DiscordClient.Guilds)
            await ChannelSynchronization.InitChannelsAsync(guild, channels);
        await repository.CommitAsync();
    }

    private async Task ProcessUsersInitializationAsync(GrillBotRepository repository)
    {
        var dbUsers = await repository.GuildUser.GetAllUsersAsync();
        dbUsers.ForEach(o => o.User!.Status = UserStatus.Offline);

        foreach (var guild in DiscordClient.Guilds)
            await GuildUserSynchronization.InitUsersAsync(guild, dbUsers);
        await repository.CommitAsync();
    }

    private async Task ProcessBotAdminInitialization(GrillBotRepository repository)
    {
        await UserSynchronization.InitBotAdminAsync(repository, await DiscordClient.GetApplicationInfoAsync());
        await repository.CommitAsync();
    }
}
