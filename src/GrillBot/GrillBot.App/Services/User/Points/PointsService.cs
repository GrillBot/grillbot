using GrillBot.App.Infrastructure;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.User.Points;

[Initializable]
public partial class PointsService
{
    private IConfiguration Configuration { get; }
    private RandomizationManager Random { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public PointsService(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, RandomizationManager random, ITextsManager texts)
    {
        Configuration = configuration.GetSection("Points");
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        Random = random;
    }

    private PointsTransaction CreateTransaction(GuildUser user, string reactionId, ulong messageId, bool ignoreCooldown)
    {
        var isReaction = !string.IsNullOrEmpty(reactionId);
        var cooldown = Configuration.GetValue<int>($"Cooldown:{(isReaction ? "Reaction" : "Message")}");
        var range = Configuration.GetSection($"Range:{(isReaction ? "Reaction" : "Message")}");

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
}
