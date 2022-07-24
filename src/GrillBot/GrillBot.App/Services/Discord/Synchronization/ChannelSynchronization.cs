using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Discord.Synchronization;

public class ChannelSynchronization : SynchronizationBase
{
    public ChannelSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task ChannelDeletedAsync(ITextChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbChannel = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id);
        if (dbChannel == null) return;

        dbChannel.Update(channel);
        dbChannel.MarkDeleted(true);
        dbChannel.RolePermissionsCount = 0;
        dbChannel.UserPermissionsCount = 0;

        if (channel is not IThreadChannel)
        {
            var threads = await repository.Channel.GetChildChannelsAsync(channel.Id, channel.Guild.Id);
            threads.ForEach(o => o.MarkDeleted(true));
        }

        await repository.CommitAsync();
    }

    public async Task ThreadDeletedAsync(IThreadChannel threadChannel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(threadChannel);
        if (thread == null) return;

        thread.Update(threadChannel);
        thread.MarkDeleted(true);
        await repository.CommitAsync();
    }

    public async Task ThreadUpdatedAsync(IThreadChannel after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(after);
        if (thread == null) return;

        thread.Update(after);
        await repository.CommitAsync();
    }

    public async Task ChannelUpdatedAsync(ITextChannel after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(after.Id, after.GuildId);
        if (channel == null) return;

        channel.Update(after);
        await repository.CommitAsync();
    }

    public static async Task InitChannelsAsync(IGuild guild, List<GuildChannel> databaseChannels)
    {
        var guildChannels = databaseChannels.FindAll(o => o.GuildId == guild.Id.ToString());

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
            if(channel == null) continue;

            channel.Update(threadChannel);
            foreach (var userStatistics in channel.Users.Where(o => o.FirstMessageAt == DateTime.MinValue))
                userStatistics.FirstMessageAt = threadChannel.CreatedAt.LocalDateTime;
        }
    }
}
