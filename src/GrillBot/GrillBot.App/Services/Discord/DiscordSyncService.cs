﻿using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;

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
}
