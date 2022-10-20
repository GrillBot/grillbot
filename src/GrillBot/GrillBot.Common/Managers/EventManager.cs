using CircularBuffer;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using IResult = Discord.Interactions.IResult;

namespace GrillBot.Common.Managers;

public class EventManager
{
    private readonly object _locker = new();

    private DiscordSocketClient DiscordClient { get; }
    private InteractionService InteractionService { get; }

    private CircularBuffer<string> EventLog { get; }
    private Dictionary<string, ulong> Statistics { get; }

    public EventManager(DiscordSocketClient discordClient, InteractionService interactionService)
    {
        DiscordClient = discordClient;
        InteractionService = interactionService;
        EventLog = new CircularBuffer<string>(1000);
        Statistics = new Dictionary<string, ulong>();

        DiscordClient.InteractionCreated += InteractionCreated;
        InteractionService.InteractionExecuted += InteractionExecuted;
        DiscordClient.MessageReceived += MessageReceived;
        DiscordClient.ReactionAdded += (_, _, reaction) => Reaction(reaction, true);
        DiscordClient.ReactionRemoved += (_, _, reaction) => Reaction(reaction, false);
        DiscordClient.UserLeft += UserLeft;
        DiscordClient.UserJoined += UserJoined;
        DiscordClient.Ready += Ready;
        DiscordClient.UserUnbanned += UserUnbanned;
        DiscordClient.MessageUpdated += (_, msg, channel) => MessageUpdated(msg, channel);
        DiscordClient.MessageDeleted += (msg, channel) => MessageDeleted(msg.Id, channel.Id);
        DiscordClient.ChannelCreated += ChannelCreated;
        DiscordClient.ChannelDestroyed += ChannelDeleted;
        DiscordClient.ChannelUpdated += (_, channel) => ChannelUpdated(channel);
        DiscordClient.GuildUpdated += (_, guild) => GuildUpdated(guild);
        DiscordClient.GuildMemberUpdated += (_, user) => GuildMemberUpdated(user);
        DiscordClient.ThreadDeleted += thread => ThreadDeleted(thread.Id);
        DiscordClient.InviteCreated += InviteCreated;
        DiscordClient.JoinedGuild += JoinedGuild;
        DiscordClient.GuildAvailable += GuildAvailable;
        DiscordClient.ThreadUpdated += (_, thread) => ThreadUpdated(thread);
        DiscordClient.PresenceUpdated += (user, _, presence) => PresenceUpdated(user, presence);
    }

    private Task PresenceUpdated(IUser user, IPresence presence)
        => AddToLog(nameof(PresenceUpdated), user.GetFullName(), presence.Status.ToString());

    private Task ThreadUpdated(SocketGuildChannel thread)
        => AddToLog(nameof(ThreadUpdated), thread.Name);

    private Task GuildAvailable(SocketGuild guild)
        => AddToLog(nameof(GuildAvailable), guild.Name);

    private Task JoinedGuild(IGuild guild)
        => AddToLog(nameof(JoinedGuild), guild.Name);

    private Task InviteCreated(SocketInvite invite)
        => AddToLog(nameof(InviteCreated), invite.Code);

    private Task ThreadDeleted(ulong threadId)
        => AddToLog(nameof(ThreadDeleted), threadId.ToString());

    private Task GuildMemberUpdated(IUser user)
        => AddToLog(nameof(GuildMemberUpdated), user.GetFullName());

    private Task GuildUpdated(IGuild guild)
        => AddToLog(nameof(GuildUpdated), guild.Name);

    private Task ChannelUpdated(IChannel channel)
        => AddToLog(nameof(ChannelUpdated), channel.Name);

    private Task ChannelDeleted(SocketChannel channel)
        => AddToLog(nameof(ChannelDeleted), ((IChannel)channel).Name);

    private Task ChannelCreated(SocketChannel channel)
        => AddToLog(nameof(ChannelCreated), ((IChannel)channel).Name);

    private Task MessageDeleted(ulong msgId, ulong channelId)
        => AddToLog(nameof(MessageDeleted), $"MessageId:{msgId}", $"ChannelId:{channelId}");

    private Task MessageUpdated(SocketMessage message, IChannel channel)
        => AddToLog(nameof(MessageUpdated), $"MessageId:{message.Id}", $"Channel:{channel.Name}", $"Author:{message.Author.GetFullName()}");

    private Task UserUnbanned(SocketUser user, SocketGuild guild)
        => AddToLog(nameof(UserUnbanned), $"Guild:{guild.Name}", user.GetFullName());

    private Task Ready()
        => AddToLog(nameof(Ready));

    private Task UserJoined(SocketGuildUser user)
        => AddToLog(nameof(UserJoined), $"Guild:{user.Guild.Name}", user.GetFullName());

    private Task UserLeft(SocketGuild guild, SocketUser user)
        => AddToLog(nameof(UserLeft), $"Guild:{guild.Name}", user.GetFullName());

    private Task Reaction(SocketReaction? reaction, bool added)
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

    private Task MessageReceived(SocketMessage msg) =>
        AddToLog(nameof(MessageReceived), msg.Id.ToString(), msg.Author.GetFullName(), $"#{msg.Channel.Name}", $"ContentLength:{msg.Content.Length}", $"Attachments:{msg.Attachments.Count}");

    private Task InteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        return AddToLog(nameof(InteractionExecuted), command.Name, command.MethodName, command.Module.Name, context.User.GetFullName(), $"Guild:{context.Guild?.Name ?? "NoGuild"}",
            $"Channel:{context.Channel.Name}", result.IsSuccess.ToString(), result.ErrorReason);
    }

    private Task InteractionCreated(SocketInteraction interaction)
        => AddToLog(nameof(InteractionCreated), interaction.User.GetFullName(), interaction.Type.ToString(), $"Guild:{interaction.GuildId}");

    private Task AddToLog(string method, params string[] parameters)
    {
        lock (_locker)
        {
            var paramsData = parameters.Length == 0 ? "" : $" - {string.Join(", ", parameters)}";
            EventLog.PushFront($"{method} - {DateTime.Now.ToCzechFormat(withMiliseconds: true)}{paramsData}");

            if (!Statistics.ContainsKey(method))
                Statistics.Add(method, 0);
            Statistics[method]++;

            return Task.CompletedTask;
        }
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
