using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Handlers
{
    // Credits to Janch and Khub.
    public class ReactionHandler : ServiceBase
    {
        private IEnumerable<ReactionEventHandler> EventHandlers { get; }
        private ILogger<ReactionHandler> Logger { get; }
        private MessageCache MessageCache { get; }

        public ReactionHandler(DiscordSocketClient client, IEnumerable<ReactionEventHandler> eventHandlers, ILogger<ReactionHandler> logger,
            MessageCache messageCache) : base(client)
        {
            DiscordClient.ReactionAdded += (message, _, reaction) => OnReactionChangedAsync(message, reaction, ReactionEvents.Added);
            DiscordClient.ReactionRemoved += (message, _, reaction) => OnReactionChangedAsync(message, reaction, ReactionEvents.Removed);

            EventHandlers = eventHandlers;
            Logger = logger;
            MessageCache = messageCache;
        }

        private async Task OnReactionChangedAsync(Cacheable<IUserMessage, ulong> message, SocketReaction reaction, ReactionEvents @event)
        {
            if (DiscordClient.Status != UserStatus.Online) return;

            var msg = (message.HasValue ? message.Value : await MessageCache.GetMessageAsync(reaction.Channel, message.Id) as IUserMessage);
            if (msg == null) return;

            var user = reaction.User.IsSpecified ? reaction.User.Value : DiscordClient.GetUser(reaction.UserId);
            user ??= await DiscordClient.Rest.GetUserAsync(reaction.UserId);
            if (user == null) return;

            if (user.Id == DiscordClient.CurrentUser.Id)
                return;

            if (msg.Author.Id != reaction.UserId && msg.Author.Id != DiscordClient.CurrentUser.Id && !reaction.Emote.IsEqual(Emojis.PersonRisingHand) && !Emojis.NumberToEmojiMap.Any(o => o.Value.IsEqual(reaction.Emote)))
            {
                // Reaction added another user than message author and emote is remind emote.
                return;
            }

            foreach (var handler in EventHandlers)
            {
                try
                {
                    if (
                        (@event == ReactionEvents.Added && await handler.OnReactionAddedAsync(msg, reaction.Emote, user)) ||
                        (@event == ReactionEvents.Removed && await handler.OnReactionRemovedAsync(msg, reaction.Emote, user))
                    )
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Reaction handler {0} threw an exception when handling reaction {1} added to message {2}.",
                        handler, reaction.Emote.Name, message.Id);
                }
            }
        }
    }
}
