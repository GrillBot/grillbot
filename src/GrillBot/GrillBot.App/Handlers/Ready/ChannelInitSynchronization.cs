using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Ready;

public class ChannelInitSynchronization : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public ChannelInitSynchronization(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
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

            foreach (var categoryChannel in await guild.GetCategoriesAsync())
            {
                var category = guildChannels.Find(o => o.IsCategory() && o.ChannelId == categoryChannel.Id.ToString());

                category?.Update(categoryChannel);
            }

            foreach (var textChannel in await guild.GetTextChannelsAsync())
            {
                var channel = guildChannels.Find(o => o.IsText() && o.ChannelId == textChannel.Id.ToString());

                channel?.Update(textChannel);
            }

            foreach (var voiceChannel in await guild.GetVoiceChannelsAsync())
            {
                var channel = guildChannels.Find(o => o.IsVoice() && o.ChannelId == voiceChannel.Id.ToString());

                channel?.Update(voiceChannel);
            }

            foreach (var stageChannel in await guild.GetStageChannelsAsync())
            {
                var channel = guildChannels.Find(o => o.IsStage() && o.ChannelId == stageChannel.Id.ToString());

                channel?.Update(stageChannel);
            }

            foreach (var threadChannel in await guild.GetThreadChannelsAsync())
            {
                var channel = guildChannels.Find(o => o.IsThread() && o.ChannelId == threadChannel.Id.ToString() && o.ParentChannelId == threadChannel.CategoryId.ToString());

                channel?.Update(threadChannel);
            }
        }

        await repository.CommitAsync();
    }
}
