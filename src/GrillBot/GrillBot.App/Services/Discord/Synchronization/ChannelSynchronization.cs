using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Discord.Synchronization;

public class ChannelSynchronization : SynchronizationBase
{
    public ChannelSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    private static IQueryable<GuildChannel> GetBaseQuery(GrillBotContext context, ulong guildId)
        => context.Channels.Where(o => o.GuildId == guildId.ToString());

    public async Task ChannelDeletedAsync(ITextChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbChannel = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id);
        if (dbChannel == null) return;

        dbChannel.MarkDeleted(true);
        if (channel is not IThreadChannel)
        {
            var threads = await repository.Channel.GetChildChannelsAsync(channel);
            threads.ForEach(o => o.MarkDeleted(true));
        }

        await repository.CommitAsync();
    }

    public async Task ThreadDeletedAsync(IThreadChannel threadChannel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(threadChannel);
        if (thread == null) return;

        thread.MarkDeleted(true);
        await repository.CommitAsync();
    }

    public async Task ThreadUpdatedAsync(IThreadChannel after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(after);
        if (thread == null) return;

        thread.Name = after.Name;
        thread.MarkDeleted(after.IsArchived);

        await repository.CommitAsync();
    }

    public async Task ChannelUpdatedAsync(ITextChannel after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(after.Id, after.GuildId);
        if (channel == null) return;

        channel.MarkDeleted(false);
        channel.Name = after.Name;

        await repository.CommitAsync();
    }

    public static async Task InitChannelsAsync(IGuild guild, List<GuildChannel> databaseChannels)
    {
        var guildChannels = databaseChannels.FindAll(o => o.GuildId == guild.Id.ToString());

        foreach (var textChannel in await guild.GetTextChannelsAsync())
        {
            var channel = guildChannels.Find(o => o.IsText() && o.ChannelId == textChannel.Id.ToString());
            if (channel == null) continue;

            channel.Name = textChannel.Name;
            channel.MarkDeleted(false);
        }

        foreach (var voiceChannel in await guild.GetVoiceChannelsAsync())
        {
            var channel = guildChannels.Find(o => o.IsVoice() && o.ChannelId == voiceChannel.Id.ToString());
            if (channel == null) continue;

            channel.Name = voiceChannel.Name;
            channel.MarkDeleted(false);
        }

        foreach (var stageChannel in await guild.GetStageChannelsAsync())
        {
            var channel = guildChannels.Find(o => o.IsStage() && o.ChannelId == stageChannel.Id.ToString());
            if (channel == null) continue;

            channel.Name = stageChannel.Name;
            channel.MarkDeleted(false);
        }

        foreach (var threadChannel in await guild.GetThreadChannelsAsync())
        {
            var channel = guildChannels.Find(o => o.IsThread() && o.ChannelId == threadChannel.Id.ToString() && o.ParentChannelId == threadChannel.CategoryId.ToString());
            if (channel == null) continue;

            channel.Name = threadChannel.Name;
            channel.MarkDeleted(threadChannel.IsArchived);
        }
    }
}
