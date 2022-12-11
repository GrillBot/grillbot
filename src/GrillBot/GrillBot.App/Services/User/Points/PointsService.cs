using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Resources.Misc;
using GrillBot.Database.Entity;
using ImageMagick;

namespace GrillBot.App.Services.User.Points;

[Initializable]
public partial class PointsService
{
    private IConfiguration Configuration { get; }
    private Random Random { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    private MagickImage TrophyImage { get; }

    public PointsService(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, RandomizationService randomizationService, ProfilePictureManager profilePictureManager,
        ITextsManager texts)
    {
        Configuration = configuration.GetSection("Points");
        Random = randomizationService.GetOrCreateGenerator("Points");
        ProfilePictureManager = profilePictureManager;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        TrophyImage = new MagickImage(MiscResources.trophy, MagickFormat.Png);
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
            Points = Random.Next(range.GetValue<int>("From"), range.GetValue<int>("To")),
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
