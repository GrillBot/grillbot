using System.Collections;
using GrillBot.App.Helpers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class EmoteMessageReceivedHandler : IMessageReceivedEvent
{
    private EmoteHelper EmoteHelper { get; }
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private ITextChannel? Channel { get; set; }
    private IGuildUser? Author { get; set; }

    public EmoteMessageReceivedHandler(EmoteHelper emoteHelper, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        EmoteHelper = emoteHelper;
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IMessage message)
    {
        var supportedEmotes = await EmoteHelper.GetSupportedEmotesAsync();
        if (!Init(message, supportedEmotes)) return;

        var emotes = message.GetEmotesFromMessage(supportedEmotes).ToList();
        if (emotes.Count == 0) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(Channel!.Guild);
        await repository.User.GetOrCreateUserAsync(Author!);
        await repository.GuildUser.GetOrCreateGuildUserAsync(Author!);

        foreach (var emote in emotes)
        {
            var emoteEntity = await repository.Emote.GetOrCreateStatisticAsync(emote, Author!, Channel.Guild);

            emoteEntity.LastOccurence = DateTime.Now;
            emoteEntity.UseCount++;
        }

        await repository.CommitAsync();
    }

    private bool Init(IMessage message, ICollection supportedEmotes)
    {
        if (supportedEmotes.Count == 0) return false; // Ignore events when no supported emotes is available.
        if (!message.TryLoadMessage(out var msg)) return false; // Ignore messages from bots.
        if (string.IsNullOrEmpty(msg?.Content)) return false; // Ignore empty messages.
        if (msg.IsCommand(DiscordClient.CurrentUser)) return false; // Ignore commands.

        Channel = msg.Channel as ITextChannel;
        Author = msg.Author as IGuildUser;
        return Channel != null && Author != null;
    }
}
