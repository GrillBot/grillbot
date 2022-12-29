using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Helpers;

public class PointsHelper
{
    private IConfiguration Configuration { get; }
    private IDiscordClient DiscordClient { get; }
    private RandomizationManager Random { get; }

    public PointsHelper(IConfiguration configuration, IDiscordClient discordClient, RandomizationManager random)
    {
        Configuration = configuration;
        DiscordClient = discordClient;
        Random = random;
    }

    public bool CanIncrementPoints(IMessage message)
    {
        var argPos = 0;
        var commandPrefix = Configuration.GetValue<string>("Discord:Commands:Prefix");

        if (message == null) return false;
        if (!message.Author.IsUser()) return false;
        if (string.IsNullOrEmpty(message.Content)) return false;
        if (message.Content.Length < Configuration.GetValue<int>("Points:MessageMinLength")) return false;
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, commandPrefix)) return false;
        if (message is IUserMessage userMsg && userMsg.ReferencedMessage?.IsCommand(ref argPos, DiscordClient.CurrentUser, commandPrefix) == true) return false;
        return message.Type != MessageType.ApplicationCommand && message.Type != MessageType.ContextMenuCommand;
    }

    public static bool CanIncrementPoints(User user, GuildChannel channel)
        => !user.HaveFlags(UserFlags.PointsDisabled) && channel != null && !channel.HasFlag(ChannelFlags.PointsDeactivated);

    public PointsTransaction CreateTransaction(GuildUser user, string reactionId, ulong messageId, bool ignoreCooldown)
    {
        var isReaction = !string.IsNullOrEmpty(reactionId);
        var cooldown = Configuration.GetValue<int>($"Points:Cooldown:{(isReaction ? "Reaction" : "Message")}");
        var range = Configuration.GetSection($"Points:Range:{(isReaction ? "Reaction" : "Message")}");

        var lastIncrement = isReaction ? user.LastPointsReactionIncrement : user.LastPointsMessageIncrement;
        if (!ignoreCooldown && lastIncrement.HasValue && lastIncrement.Value.AddSeconds(cooldown) > DateTime.Now)
            return null;

        var transaction = new PointsTransaction
        {
            GuildId = user.GuildId,
            Points = Random.GetNext("Points", range.GetValue<int>("From"), range.GetValue<int>("To")),
            AssingnedAt = DateTime.Now,
            ReactionId = reactionId ?? "",
            MessageId = messageId > 0 ? messageId.ToString() : SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            UserId = user.UserId
        };

        if (ignoreCooldown || messageId == 0)
            return transaction;

        if (isReaction)
            user.LastPointsReactionIncrement = transaction.AssingnedAt;
        else
            user.LastPointsMessageIncrement = transaction.AssingnedAt;
        return transaction;
    }

    public PointsTransaction CreateMigratedTransaction(GuildUser guildUser, PointsTransaction referenceTransaction)
    {
        if (referenceTransaction == null || guildUser.Points == 0) return null;

        var transaction = CreateTransaction(guildUser, referenceTransaction.ReactionId, 0, true);
        transaction.Points = referenceTransaction.Points * 100;

        guildUser.Points -= transaction.Points;
        if (guildUser.Points < 0) guildUser.Points = 0;

        return transaction;
    }

    public static async Task<List<PointsTransaction>> FilterTransactionsAsync(GrillBotRepository repository, params PointsTransaction[] transactions)
    {
        var result = new List<PointsTransaction>();

        foreach (var transaction in transactions)
        {
            if (transaction == null || transaction.Points == 0) continue;
            if (await repository.Points.ExistsTransactionAsync(transaction)) continue;

            result.Add(transaction);
        }

        return result;
    }

    public static string CreateReactionId(SocketReaction reaction)
        => $"{reaction.Emote}_{reaction.UserId}";
}
