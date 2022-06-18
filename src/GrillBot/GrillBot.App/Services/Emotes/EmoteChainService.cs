using Discord.Commands;
using GrillBot.App.Infrastructure;

namespace GrillBot.App.Services.Emotes;

[Initializable]
public class EmoteChainService
{
    // Dictionary<GuildID|ChannelID, List<UserID, Message>>
    private Dictionary<string, List<Tuple<ulong, string>>> LastMessages { get; }
    private int RequiredCount { get; }

    private readonly object _locker = new();
    private DiscordSocketClient DiscordClient { get; }

    public EmoteChainService(IConfiguration configuration, DiscordSocketClient client)
    {
        RequiredCount = configuration.GetValue<int>("Emotes:ChainRequiredCount");
        LastMessages = new Dictionary<string, List<Tuple<ulong, string>>>();
        DiscordClient = client;

        DiscordClient.MessageReceived += msg => msg.TryLoadMessage(out var message) ? OnMessageReceivedAsync(message) : Task.CompletedTask;
    }

    private void Cleanup(IGuildChannel channel)
    {
        lock (_locker)
        {
            CleanupNoLock(channel);
        }
    }

    private void CleanupNoLock(IGuildChannel channel)
    {
        var key = GetKey(channel);
        if (LastMessages.ContainsKey(key)) LastMessages[key].Clear();
    }

    private Task OnMessageReceivedAsync(IUserMessage message)
    {
        if (RequiredCount < 1) return Task.CompletedTask;
        var context = new CommandContext(DiscordClient, message);

        return context.IsPrivate ? Task.CompletedTask : ProcessChainAsync(context);
    }

    private async Task ProcessChainAsync(ICommandContext context)
    {
        if (context.Channel is not ITextChannel channel) return;
        var author = context.Message.Author;
        var content = context.Message.Content;
        var key = GetKey(channel);

        if (!LastMessages.ContainsKey(key))
            LastMessages.Add(key, new List<Tuple<ulong, string>>(RequiredCount));

        if (!IsValidMessage(context.Message, context.Guild, channel))
        {
            Cleanup(channel);
            return;
        }

        var group = LastMessages[key];

        if (group.All(o => o.Item1 != author.Id))
            group.Add(new Tuple<ulong, string>(author.Id, content));

        if (group.Count == RequiredCount)
        {
            await channel.SendMessageAsync(group[0].Item2);
            Cleanup(channel);
        }
    }

    private bool IsValidWithWithFirstInChannel(IGuildChannel channel, string content)
    {
        var key = GetKey(channel);
        var group = LastMessages[key];

        if (group.Count == 0)
            return true;

        return content == group[0].Item2;
    }

    private bool IsValidMessage(IMessage message, IGuild guild, IGuildChannel channel)
    {
        var emotes = message.Tags
            .Where(o => o.Type == TagType.Emoji && guild.Emotes.Any(x => x.Id == o.Key))
            .ToList();

        var isUtfEmoji = NeoSmart.Unicode.Emoji.IsEmoji(message.Content);
        if (emotes.Count == 0 && !isUtfEmoji) return false;

        if (!IsValidWithWithFirstInChannel(channel, message.Content)) return false;
        var emoteTemplate = string.Join(" ", emotes.Select(o => o.Value.ToString()));
        return emoteTemplate == message.Content || isUtfEmoji;
    }

    private static string GetKey(IGuildChannel channel) => $"{channel.Guild.Id}|{channel.Id}";
}
