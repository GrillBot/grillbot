using GrillBot.App.Infrastructure;
using GrillBot.Common;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindPostponeReactionHandler : ReactionEventHandler
{
    private GrillBotDatabaseBuilder DbFactory { get; }
    private IDiscordClient DiscordClient { get; }

    public RemindPostponeReactionHandler(GrillBotDatabaseBuilder dbFactory, IDiscordClient discordClient)
    {
        DbFactory = dbFactory;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (message.Channel is not IDMChannel) return false; // In DM
        if (message.Embeds.Count != 1) return false; // Contains embed
        if (emote is not Emoji) return false; // Is Emoji

        var hoursMove = Emojis.NumberToEmojiMap
            .Where(o => o.Key > 0)
            .FirstOrDefault(o => o.Value.IsEqual(emote)).Key;
        if (hoursMove == default) return false; // Not known emoji.

        var reactions = await message.GetReactionUsersAsync(emote, 5).FlattenAsync();
        if (!reactions.Any(o => o.IsBot && o.Id == DiscordClient.CurrentUser.Id)) return false; // Message contains reaction from bot.

        await using var repository = DbFactory.CreateRepository();

        var remind = await repository.Remind.FindRemindByRemindMessageAsync(message.Id.ToString());
        if (remind == null) return false; // Remind message not found or not triggered.

        remind.RemindMessageId = null;
        remind.At = DateTime.Now.AddHours(hoursMove);
        remind.Postpone++;

        await message.DeleteAsync();
        await repository.CommitAsync();
        return true;
    }
}
