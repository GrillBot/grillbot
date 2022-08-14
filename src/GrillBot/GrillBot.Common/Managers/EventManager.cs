using CircularBuffer;
using Discord;
using Discord.Commands;
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
    private CommandService CommandService { get; }
    private CircularBuffer<string> EventLog { get; }

    public EventManager(DiscordSocketClient discordClient, InteractionService interactionService, CommandService commandService)
    {
        DiscordClient = discordClient;
        InteractionService = interactionService;
        CommandService = commandService;
        EventLog = new CircularBuffer<string>(1000);

        DiscordClient.InteractionCreated += OnInteractionCreated;
        InteractionService.InteractionExecuted += OnInteractionExecuted;
        CommandService.CommandExecuted += OnCommandExecuted;
        DiscordClient.MessageReceived += OnMessageReceived;
        DiscordClient.ReactionAdded += (_, _, reaction) => OnReaction(reaction, true);
        DiscordClient.ReactionRemoved += (_, _, reaction) => OnReaction(reaction, false);
        DiscordClient.UserLeft += OnUserLeft;
        DiscordClient.UserJoined += OnUserJoined;
        DiscordClient.Ready += OnReady;
        DiscordClient.UserUnbanned += OnUserUnbanned;
        DiscordClient.MessageUpdated += (_, msg, channel) => OnMessageUpdated(msg, channel);
        DiscordClient.MessageDeleted += (msg, channel) => OnMessageDeleted(msg.Id, channel.Id);
        DiscordClient.ChannelCreated += OnChannelCreated;
        DiscordClient.ChannelDestroyed += OnChannelDeleted;
        DiscordClient.ChannelUpdated += (_, channel) => OnChannelUpdated(channel);
        DiscordClient.GuildUpdated += (_, guild) => OnGuildUpdated(guild);
        DiscordClient.GuildMemberUpdated += (_, user) => OnGuildMemberUpdated(user);
        DiscordClient.ThreadDeleted += thread => OnThreadDeleted(thread.Id);
        DiscordClient.InviteCreated += OnInviteCreated;
        DiscordClient.JoinedGuild += OnJoinedGuild;
        DiscordClient.GuildAvailable += OnGuildAvailable;
        DiscordClient.ThreadUpdated += (_, thread) => OnThreadUpdated(thread);
        DiscordClient.PresenceUpdated += (user, _, presence) => OnPresenceUpdated(user, presence);
    }

    private Task OnPresenceUpdated(IUser user, IPresence presence)
        => AddToLog(nameof(OnPresenceUpdated), user.GetFullName(), presence.Status.ToString());

    private Task OnThreadUpdated(SocketGuildChannel thread)
        => AddToLog(nameof(OnThreadUpdated), thread.Name);

    private Task OnGuildAvailable(SocketGuild guild)
        => AddToLog(nameof(OnGuildAvailable), guild.Name);

    private Task OnJoinedGuild(IGuild guild)
        => AddToLog(nameof(OnJoinedGuild), guild.Name);

    private Task OnInviteCreated(SocketInvite invite)
        => AddToLog(nameof(OnInviteCreated), invite.Code);

    private Task OnThreadDeleted(ulong threadId)
        => AddToLog(nameof(OnThreadDeleted), threadId.ToString());

    private Task OnGuildMemberUpdated(IUser user)
        => AddToLog(nameof(OnGuildMemberUpdated), user.GetFullName());

    private Task OnGuildUpdated(IGuild guild)
        => AddToLog(nameof(OnGuildUpdated), guild.Name);

    private Task OnChannelUpdated(IChannel channel)
        => AddToLog(nameof(OnChannelDeleted), channel.Name);

    private Task OnChannelDeleted(SocketChannel channel)
        => AddToLog(nameof(OnChannelDeleted), ((IChannel)channel).Name);

    private Task OnChannelCreated(SocketChannel channel)
        => AddToLog(nameof(OnChannelCreated), ((IChannel)channel).Name);

    private Task OnMessageDeleted(ulong msgId, ulong channelId)
        => AddToLog(nameof(OnMessageDeleted), $"MessageId:{msgId}", $"ChannelId:{channelId}");

    private Task OnMessageUpdated(SocketMessage message, IChannel channel)
        => AddToLog(nameof(OnMessageUpdated), $"MessageId:{message.Id}", $"Channel:{channel.Name}", $"Author:{message.Author.GetFullName()}");

    private Task OnUserUnbanned(SocketUser user, SocketGuild guild)
        => AddToLog(nameof(OnUserUnbanned), $"Guild:{guild.Name}", user.GetFullName());

    private Task OnReady()
        => AddToLog(nameof(OnReady));

    private Task OnUserJoined(SocketGuildUser user)
        => AddToLog(nameof(OnUserJoined), $"Guild:{user.Guild.Name}", user.GetFullName());

    private Task OnUserLeft(SocketGuild guild, SocketUser user)
        => AddToLog(nameof(OnUserLeft), $"Guild:{guild.Name}", user.GetFullName());

    private Task OnReaction(SocketReaction reaction, bool added)
    {
        var parameters = new[]
        {
            $"MessageId:{reaction.MessageId}",
            $"Channel:{reaction.Channel.Name}",
            $"Emote:{reaction.Emote}",
            reaction.User.IsSpecified ? reaction.User.Value.GetFullName() : $"UserId:{reaction.UserId}",
            $"IsAdded:{added}"
        };

        return AddToLog(nameof(OnReaction), parameters);
    }

    private Task OnMessageReceived(SocketMessage msg) =>
        AddToLog(nameof(OnMessageReceived), msg.Id.ToString(), msg.Author.GetFullName(), $"#{msg.Channel.Name}", $"ContentLength:{msg.Content.Length}", $"Attachments:{msg.Attachments.Count}");

    private Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
    {
        if (!command.IsSpecified) return Task.CompletedTask;

        return AddToLog(nameof(OnCommandExecuted), command.Value.Name, command.Value.Module.Name, context.User.GetFullName(), $"Guild:{context.Guild?.Name ?? "NoGuild"}",
            $"Channel:{context.Channel.Name}", result.IsSuccess.ToString(), result.ErrorReason);
    }

    private Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        return AddToLog(nameof(OnInteractionExecuted), command.Name, command.MethodName, command.Module.Name, context.User.GetFullName(), $"Guild:{context.Guild?.Name ?? "NoGuild"}",
            $"Channel:{context.Channel.Name}", result.IsSuccess.ToString(), result.ErrorReason);
    }

    private Task OnInteractionCreated(SocketInteraction interaction)
        => AddToLog(nameof(OnInteractionCreated), interaction.User.GetFullName(), interaction.Type.ToString(), $"Guild:{interaction.GuildId}");

    private Task AddToLog(string method, params string[] parameters)
    {
        lock (_locker)
        {
            var paramsData = parameters.Length == 0 ? "" : $" - {string.Join(", ", parameters)}";
            EventLog.PushFront($"{method} - {DateTime.Now.ToCzechFormat(withMiliseconds:true)}{paramsData}");
            return Task.CompletedTask;
        }
    }

    public string[] GetData()
    {
        lock (_locker)
        {
            return EventLog.ToArray();
        }
    }
}
