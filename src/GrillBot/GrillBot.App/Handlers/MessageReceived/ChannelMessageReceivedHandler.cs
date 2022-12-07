using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class ChannelMessageReceivedHandler : IMessageReceivedEvent
{
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelMessageReceivedHandler(IDiscordClient discordClient, IConfiguration configuration, GrillBotDatabaseBuilder databaseBuilder)
    {
        DiscordClient = discordClient;
        Configuration = configuration;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!Init(message, out var textChannel, out var author)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(textChannel.Guild);
        await repository.User.GetOrCreateUserAsync(author);
        await repository.GuildUser.GetOrCreateGuildUserAsync(author);
        await repository.Channel.GetOrCreateChannelAsync(textChannel);

        var userChannel = await repository.Channel.GetOrCreateUserChannelAsync(textChannel, author);

        userChannel.Count++;
        userChannel.LastMessageAt = DateTime.Now;
        await repository.CommitAsync();
    }

    private bool Init(IMessage message, out ITextChannel textChannel, out IGuildUser author)
    {
        textChannel = message.Channel as ITextChannel;
        author = message.Author as IGuildUser;

        var argPos = 0;
        var commandPrefix = Configuration.GetValue<string>("Discord:Commands:Prefix");
        return !message.IsCommand(ref argPos, DiscordClient.CurrentUser, commandPrefix) && textChannel != null && author != null;
    }
}
