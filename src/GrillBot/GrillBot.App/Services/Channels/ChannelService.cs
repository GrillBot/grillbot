using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Services.Channels;

[Initializable]
public class ChannelService
{
    private string CommandPrefix { get; }
    private IMessageCacheManager MessageCache { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration,
        IMessageCacheManager messageCache)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        MessageCache = messageCache;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.MessageReceived += message => message.TryLoadMessage(out var msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        var argPos = 0;

        // Commands and DM in channelboard is not allowed.
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return;
        if (message.Channel is not ITextChannel textChannel) return;
        if (message.Author is not IGuildUser author) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateRepositoryAsync(textChannel.Guild);
        await repository.User.GetOrCreateUserAsync(author);

        var channel = await repository.Channel.GetOrCreateChannelAsync(textChannel, ChannelsIncludeUsersMode.IncludeAll);
        var user = await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        var userChannel = channel.Users.FirstOrDefault(o => o.UserId == user.UserId);
        if (userChannel == null)
        {
            userChannel = new Database.Entity.GuildUserChannel
            {
                User = user,
                FirstMessageAt = DateTime.Now,
                Count = 0
            };

            channel.Users.Add(userChannel);
        }

        userChannel.Count++;
        userChannel.LastMessageAt = DateTime.Now;
        await repository.CommitAsync();
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, null, true);
        if (!messageChannel.HasValue || msg == null || messageChannel.Value is not ITextChannel channel) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelEntity = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.GuildId, true);
        var userChannel = channelEntity?.Users.FirstOrDefault(o => o.UserId == msg.Author.Id.ToString());
        if (userChannel == null) return;

        userChannel.Count--;
        await repository.CommitAsync();
    }
}
