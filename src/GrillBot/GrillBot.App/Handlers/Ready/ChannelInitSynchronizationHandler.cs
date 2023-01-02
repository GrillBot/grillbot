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

        await using var repository = DatabaseBuilder.CreateRepository();

        var channels = await repository.Channel.GetAllChannelsAsync();
        foreach (var channel in channels)
        {
            channel.MarkDeleted(true);
            channel.RolePermissionsCount = 0;
            channel.UserPermissionsCount = 0;
        }

        foreach (var guild in guilds)
        {
            var guildChannels = channels.FindAll(o => o.GuildId == guild.Id.ToString());

            foreach (var channel in await guild.GetChannelsAsync())
            {
                var dbChannel = guildChannels.Find(o => o.ChannelId == channel.Id.ToString());

                dbChannel?.Update(channel);
            }
        }

        await repository.CommitAsync();
    }
}
