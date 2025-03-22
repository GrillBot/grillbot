using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class ChannelMessageReceivedHandler : IMessageReceivedEvent
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private ITextChannel? Channel { get; set; }
    private IGuildUser? Author { get; set; }

    public ChannelMessageReceivedHandler(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!Init(message)) return;

        using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(Channel!.Guild);
        await repository.User.GetOrCreateUserAsync(Author!);
        await repository.GuildUser.GetOrCreateGuildUserAsync(Author!);
        await repository.Channel.GetOrCreateChannelAsync(Channel);

        var userChannel = await repository.Channel.GetOrCreateUserChannelAsync(Channel, Author!);

        userChannel.Count++;
        userChannel.LastMessageAt = DateTime.Now;
        await repository.CommitAsync();
    }

    private bool Init(IMessage message)
    {
        Channel = message.Channel as ITextChannel;
        Author = message.Author as IGuildUser;

        return !message.IsCommand(DiscordClient.CurrentUser) && Channel != null && Author != null;
    }
}
