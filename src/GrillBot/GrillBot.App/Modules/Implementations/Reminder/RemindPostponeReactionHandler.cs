using GrillBot.App.Infrastructure;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindPostponeReactionHandler : ReactionEventHandler
{
    private GrillBotDatabaseFactory DbFactory { get; }
    private IDiscordClient DiscordClient { get; }

    public RemindPostponeReactionHandler(GrillBotDatabaseFactory dbFactory, IDiscordClient discordClient)
    {
        DbFactory = dbFactory;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (message.Channel is not IDMChannel) return false; // In DM
        if (message.Embeds.Count != 1) return false; // Contains embed
        if (emote is not Emoji emoji) return false; // Is Emoji

        var hoursMove = Emojis.NumberToEmojiMap
            .Where(o => o.Key > 0)
            .FirstOrDefault(o => o.Value.IsEqual(emote)).Key;
        if (hoursMove == default) return false; // Not known emoji.

        var reactions = await message.GetReactionUsersAsync(emote, 5).FlattenAsync();
        if (!reactions.Any(o => o.IsBot && o.Id == DiscordClient.CurrentUser.Id)) return false; // Message contains reaction from bot.

        using var context = DbFactory.Create();

        var remind = await context.Reminders.AsQueryable()
            .FirstOrDefaultAsync(o => o.RemindMessageId == message.Id.ToString() && o.At < DateTime.Now);

        if (remind == null) return false; // Remind message not found or not triggered.

        remind.RemindMessageId = null;
        remind.At = DateTime.Now.AddHours(hoursMove);
        remind.Postpone++;

        await message.DeleteAsync();
        await context.SaveChangesAsync();
        return true;
    }
}
