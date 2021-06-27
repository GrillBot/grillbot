using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Reminder;
using GrillBot.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules.Reminder
{
    public class RemindReactionHandler : ReactionEventHandler
    {
        private RemindService RemindService { get; }
        private DiscordSocketClient DiscordClient { get; }

        public RemindReactionHandler(RemindService remindService, DiscordSocketClient discordClient)
        {
            RemindService = remindService;
            DiscordClient = discordClient;
        }

        public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
        {
            if (!TryGetEmbedAndMetadata<RemindListMetadata>(message, emote, out IEmbed embed, out var metadata))
            {
                if (emote is Emoji && emote.IsEqual(Emojis.PersonRisingHand))
                {
                    try
                    {
                        await RemindService.CopyAsync(message, user);
                    }
                    catch (ValidationException ex)
                    {
                        await message.Channel.SendMessageAsync($"{user.Mention} {ex.Message}");
                    }

                    return true;
                }

                return false;
            }

            var forUser = await DiscordClient.FindUserAsync(metadata.OfUser);
            if (forUser == null) return false;

            var remindsCount = await RemindService.GetRemindersCountAsync(forUser);
            if (remindsCount == 0) return false;
            var pagesCount = (int)Math.Ceiling(remindsCount / (double)EmbedBuilder.MaxFieldCount);

            var newPage = GetPageNumber(metadata.Page, pagesCount, emote);
            if (newPage == metadata.Page) return false;

            var reminders = await RemindService.GetRemindersAsync(user, newPage);
            if (reminders.Count == 0) return false;

            var resultEmbed = await new EmbedBuilder()
                .WithRemindListAsync(reminders, DiscordClient, user, user, newPage);
            await message.ModifyAsync(o => o.Embed = resultEmbed.Build());

            var context = new CommandContext(DiscordClient, message);
            if (!context.IsPrivate)
                await message.RemoveReactionAsync(emote, user);

            return true;
        }
    }
}
