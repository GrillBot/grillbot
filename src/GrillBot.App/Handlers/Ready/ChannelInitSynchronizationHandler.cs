using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Ready;

public class ChannelInitSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public ChannelInitSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();

        using var repository = DatabaseBuilder.CreateRepository();

        var channels = await repository.Channel.GetAllChannelsAsync();
        foreach (var channel in channels)
        {
            channel.MarkDeleted(true);
            channel.RolePermissionsCount = 0;
            channel.UserPermissionsCount = 0;
        }

        foreach (var guild in guilds)
        {
            var guildChannels = channels
                .Where(o => o.GuildId == guild.Id.ToString())
                .ToDictionary(o => o.ChannelId, o => o);

            foreach (var channel in await guild.GetChannelsAsync())
            {
                if (guildChannels.TryGetValue(channel.Id.ToString(), out var dbChannel))
                    dbChannel.Update(channel);
            }
        }

        await repository.CommitAsync();
    }
}
