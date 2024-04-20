using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Handlers.Ready;

public class GuildSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public GuildSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();

        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var guild in guilds)
        {
            var dbGuild = await repository.Guild.FindGuildAsync(guild);
            if (dbGuild is null)
                continue;

            if (await CanResetChannelIdAsync(dbGuild.AdminChannelId, guild))
                dbGuild.AdminChannelId = null;
            if (await CanResetChannelIdAsync(dbGuild.BotRoomChannelId, guild))
                dbGuild.BotRoomChannelId = null;
            if (await CanResetChannelIdAsync(dbGuild.VoteChannelId, guild))
                dbGuild.VoteChannelId = null;
            if (CanResetRoleId(dbGuild.MuteRoleId, guild))
                dbGuild.MuteRoleId = null;
            if (CanResetRoleId(dbGuild.AssociationRoleId, guild))
                dbGuild.AssociationRoleId = null;
        }

        await repository.CommitAsync();
    }

    private static async Task<bool> CanResetChannelIdAsync(string? expectedChannelId, IGuild guild)
    {
        if (string.IsNullOrEmpty(expectedChannelId))
            return false;

        var channel = await guild.GetChannelAsync(expectedChannelId.ToUlong());
        return channel is null;
    }

    private static bool CanResetRoleId(string? expectedRoleId, IGuild guild)
    {
        if (string.IsNullOrEmpty(expectedRoleId))
            return false;

        var role = guild.GetRole(expectedRoleId.ToUlong());
        return role is null;
    }
}
