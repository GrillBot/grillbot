using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Random;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Helpers;

public class PointsHelper
{
    private IConfiguration Configuration { get; }
    private IDiscordClient DiscordClient { get; }
    private IRandomManager Random { get; }

    public PointsHelper(IConfiguration configuration, IDiscordClient discordClient, IRandomManager random)
    {
        Configuration = configuration;
        DiscordClient = discordClient;
        Random = random;
    }

    public bool CanIncrementPoints(IMessage? message)
    {
        if (message == null) return false;
        if (!message.Author.IsUser()) return false;
        if (string.IsNullOrEmpty(message.Content)) return false;
        if (message.Content.Length < Configuration.GetValue<int>("Points:MessageMinLength")) return false;
        if (message.IsCommand(DiscordClient.CurrentUser)) return false;
        if (message is IUserMessage userMsg && userMsg.ReferencedMessage?.IsCommand(DiscordClient.CurrentUser) == true) return false;
        return message.Type != MessageType.ApplicationCommand && message.Type != MessageType.ContextMenuCommand;
    }

    public static bool CanIncrementPoints(User user, GuildChannel? channel)
        => !user.HaveFlags(UserFlags.PointsDisabled) && channel != null && !channel.HasFlag(ChannelFlag.PointsDeactivated);

    public PointsTransaction? CreateTransaction(GuildUser user, string? reactionId, ulong messageId, bool ignoreCooldown)
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

    public static async Task<bool> CanStoreTransactionAsync(GrillBotRepository repository, PointsTransaction? transaction)
        => transaction is { Points: > 0 } && !await repository.Points.ExistsTransactionAsync(transaction);

    public static async Task<List<PointsTransaction>> FilterTransactionsAsync(GrillBotRepository repository, params PointsTransaction?[] transactions)
    {
        var result = await transactions
            .FindAllAsync(async o => await CanStoreTransactionAsync(repository, o));

        return result.ConvertAll(o => o!);
    }

    public static string CreateReactionId(SocketReaction reaction)
        => $"{reaction.Emote}_{reaction.UserId}";
}
