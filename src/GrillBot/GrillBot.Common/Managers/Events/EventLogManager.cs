using CircularBuffer;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using IResult = Discord.Interactions.IResult;

namespace GrillBot.Common.Managers.Events;

public class EventLogManager
{
    private readonly object _locker = new();

    private CircularBuffer<string> EventLog { get; }
    private Dictionary<string, ulong> Statistics { get; }

    public EventLogManager()
    {
        EventLog = new CircularBuffer<string>(1000);
        Statistics = new Dictionary<string, ulong>();
    }

    public Task ThreadUpdated(SocketGuildChannel thread) => AddToLog(nameof(ThreadUpdated), thread.Name);
    public Task GuildAvailable(SocketGuild guild) => AddToLog(nameof(GuildAvailable), guild.Name);
    public Task JoinedGuild(IGuild guild) => AddToLog(nameof(JoinedGuild), guild.Name);
    public Task InviteCreated(SocketInvite invite) => AddToLog(nameof(InviteCreated), invite.Code);
    public Task ThreadDeleted(ulong threadId) => AddToLog(nameof(ThreadDeleted), threadId.ToString());
    public Task GuildMemberUpdated(IUser user) => AddToLog(nameof(GuildMemberUpdated), user.GetFullName());
    public Task GuildUpdated(IGuild guild) => AddToLog(nameof(GuildUpdated), guild.Name);
    public Task ChannelUpdated(IChannel channel) => AddToLog(nameof(ChannelUpdated), channel.Name);
    public Task ChannelDeleted(SocketChannel channel) => AddToLog(nameof(ChannelDeleted), ((IChannel)channel).Name);
    public Task ChannelCreated(SocketChannel channel) => AddToLog(nameof(ChannelCreated), ((IChannel)channel).Name);
    public Task MessageDeleted(ulong msgId, ulong channelId) => AddToLog(nameof(MessageDeleted), $"MessageId:{msgId}", $"ChannelId:{channelId}");

    public Task MessageUpdated(SocketMessage message, IChannel channel) =>
        AddToLog(nameof(MessageUpdated), $"MessageId:{message.Id}", $"Channel:{channel.Name}", $"Author:{message.Author.GetFullName()}");

    public Task UserUnbanned(SocketUser user, SocketGuild guild) => AddToLog(nameof(UserUnbanned), $"Guild:{guild.Name}", user.GetFullName());
    public Task Ready() => AddToLog(nameof(Ready));
    public Task UserJoined(SocketGuildUser user) => AddToLog(nameof(UserJoined), $"Guild:{user.Guild.Name}", user.GetFullName());
    public Task UserLeft(SocketGuild guild, SocketUser user) => AddToLog(nameof(UserLeft), $"Guild:{guild.Name}", user.GetFullName());
    public Task UserUpdated(IUser user) => AddToLog(nameof(UserUpdated), user.GetFullName());

    public Task Reaction(SocketReaction? reaction, bool added)
    {
        var paramsBuilder = new List<string> { $"IsAdded:{added}" };
        if (reaction == null)
            return AddToLog(nameof(Reaction), paramsBuilder.ToArray());

        paramsBuilder.Add($"MessageId:{reaction.MessageId}");
        paramsBuilder.Add($"Channel:{reaction.Channel?.Name ?? "null"}");
        paramsBuilder.Add($"Emote:{reaction.Emote}");
        paramsBuilder.Add(reaction.User.IsSpecified ? reaction.User.Value.GetFullName() : $"UserId:{reaction.UserId}");
        return AddToLog(nameof(Reaction), paramsBuilder.ToArray());
    }

    public Task MessageReceived(SocketMessage msg) => AddToLog(nameof(MessageReceived), msg.Id.ToString(), msg.Author.GetFullName(), $"#{msg.Channel.Name}", $"ContentLength:{msg.Content.Length}",
        $"Attachments:{msg.Attachments.Count}");

    public Task InteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result) => AddToLog(nameof(InteractionExecuted), command.Name, command.MethodName, command.Module.Name,
        context.User.GetFullName(), $"Guild:{context.Guild?.Name ?? "NoGuild"}", $"Channel:{context.Channel.Name}", result.IsSuccess.ToString(), result.ErrorReason);

    private Task AddToLog(string method, params string[] parameters)
    {
        var paramsData = parameters.Length == 0 ? "" : $" - {string.Join(", ", parameters)}";
        var eventLogItem = $"{method} - {DateTime.Now.ToCzechFormat(withMiliseconds: true)}{paramsData}";

        lock (_locker)
        {
            EventLog.PushFront(eventLogItem);

            if (!Statistics.ContainsKey(method))
                Statistics.Add(method, 0);
            Statistics[method]++;
        }

        return Task.CompletedTask;
    }

    public string[] GetEventLog()
    {
        lock (_locker)
        {
            return EventLog.ToArray();
        }
    }

    public Dictionary<string, ulong> GetStatistics()
    {
        lock (_locker)
        {
            return Statistics
                .OrderByDescending(o => o.Value).ThenBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Value);
        }
    }
}
