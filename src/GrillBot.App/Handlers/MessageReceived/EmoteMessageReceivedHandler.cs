using System.Collections;
using GrillBot.App.Helpers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class EmoteMessageReceivedHandler : IMessageReceivedEvent
{
    private EmoteHelper EmoteHelper { get; }
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteMessageReceivedHandler(EmoteHelper emoteHelper, IDiscordClient discordClient, IConfiguration configuration, GrillBotDatabaseBuilder databaseBuilder)
    {
        EmoteHelper = emoteHelper;
        DiscordClient = discordClient;
        Configuration = configuration;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IMessage message)
    {
        var supportedEmotes = await EmoteHelper.GetSupportedEmotesAsync();
        if (!Init(message, supportedEmotes, out var channel, out var author)) return;

        var emotes = message.GetEmotesFromMessage(supportedEmotes).ToList();
        if (emotes.Count == 0) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(channel.Guild);
        await repository.User.GetOrCreateUserAsync(author);
        await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        foreach (var emote in emotes)
        {
            var emoteEntity = await repository.Emote.GetOrCreateStatisticAsync(emote, author, channel.Guild);

            emoteEntity.LastOccurence = DateTime.Now;
            emoteEntity.UseCount++;
        }

        await repository.CommitAsync();
    }

    private bool Init(IMessage message, ICollection supportedEmotes, out ITextChannel channel, out IGuildUser author)
    {
        channel = null;
        author = null;

        if (supportedEmotes.Count == 0) return false; // Ignore events when no supported emotes is available.
        if (!message.TryLoadMessage(out var msg)) return false; // Ignore messages from bots.
        if (string.IsNullOrEmpty(msg?.Content)) return false; // Ignore empty messages.

        var prefix = Configuration.GetValue<string>("Discord:Commands:Prefix");
        if (msg.IsCommand(DiscordClient.CurrentUser, prefix)) return false; // Ignore commands.

        channel = msg.Channel as ITextChannel;
        author = msg.Author as IGuildUser;
        return channel != null && author != null;
    }
}
