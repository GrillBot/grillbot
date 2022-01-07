using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord;
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
            MessageCache messageCache, DiscordInitializationService initializationService) : base(client, null, initializationService)
        {
            DiscordClient.ReactionAdded += (message, channel, reaction) => OnReactionChangedAsync(message, reaction, ReactionEvents.Added, channel);
            DiscordClient.ReactionRemoved += (message, channel, reaction) => OnReactionChangedAsync(message, reaction, ReactionEvents.Removed, channel);

            EventHandlers = eventHandlers;
            Logger = logger;
            MessageCache = messageCache;
        }

        private async Task OnReactionChangedAsync(Cacheable<IUserMessage, ulong> message, SocketReaction reaction, ReactionEvents @event, Cacheable<IMessageChannel, ulong> channel)
        {
            if (!InitializationService.Get()) return;

            var messageChannel = await channel.GetOrDownloadAsync();
            if (messageChannel == null) return;

            var msg = (message.HasValue ? message.Value : await MessageCache.GetMessageAsync(messageChannel, message.Id) as IUserMessage);
            if (msg == null) return;

            var user = reaction.User.IsSpecified ? reaction.User.Value : DiscordClient.GetUser(reaction.UserId);
            user ??= await DiscordClient.Rest.GetUserAsync(reaction.UserId);
            if (user == null) return;

            if (user.Id == DiscordClient.CurrentUser.Id)
                return;

            if (msg.Author.Id != DiscordClient.CurrentUser.Id)
            {
                if (messageChannel is IDMChannel)
                {
                    // Only numbers are allowed in dms
                    if (!Emojis.NumberToEmojiMap.Any(o => o.Value.IsEqual(reaction.Emote)))
                        return;
                }
                else
                {
                    // Only remind copy is allowed.
                    if (!reaction.Emote.IsEqual(Emojis.PersonRisingHand))
                        return;
                }
            }
            else
            {
                if (Emojis.PaginationEmojis.Any(o => o.IsEqual(reaction.Emote)))
                {
                    // Restrict pagination only on author.
                    if (msg.ReferencedMessage == null) return;
                    if (msg.ReferencedMessage.Author.Id != reaction.UserId) return;
                }
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
