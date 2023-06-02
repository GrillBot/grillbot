using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Managers.Logging;
using GrillBot.Database.Entity;

namespace GrillBot.App.Handlers.Ready;

public class ChannelInitSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private LoggingManager LoggingManager { get; }

    public ChannelInitSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, LoggingManager loggingManager)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        LoggingManager = loggingManager;
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
            channel.PinCount = 0;
        }

        foreach (var guild in guilds)
        {
            var guildChannels = channels.FindAll(o => o.GuildId == guild.Id.ToString());

            foreach (var channel in await guild.GetChannelsAsync())
            {
                var dbChannel = guildChannels.Find(o => o.ChannelId == channel.Id.ToString());
                if (dbChannel is null)
                    continue;

                dbChannel.Update(channel);
                if (channel is IVoiceChannel || channel is not ITextChannel textChannel)
                    continue;

                await UpdatePinCountAsync(textChannel, dbChannel);
            }
        }

        await repository.CommitAsync();
    }

    private async Task UpdatePinCountAsync(IMessageChannel textChannel, GuildChannel dbChannel)
    {
        try
        {
            var pins = await textChannel.GetPinnedMessagesAsync();
            dbChannel.PinCount = pins.Count;
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync(nameof(ChannelInitSynchronizationHandler), "An error occured while processing initial channel synchronization.", ex);
        }
    }
}
