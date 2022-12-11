using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Resources.Misc;
using GrillBot.Database.Entity;
using ImageMagick;

namespace GrillBot.App.Services.User.Points;

[Initializable]
public partial class PointsService
{
    private string CommandPrefix { get; }
    private IConfiguration Configuration { get; }
    private Random Random { get; }
    private IMessageCacheManager MessageCache { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    private MagickImage TrophyImage { get; }

    public PointsService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, IMessageCacheManager messageCache,
        RandomizationService randomizationService, ProfilePictureManager profilePictureManager, ITextsManager texts)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        Configuration = configuration.GetSection("Points");
        Random = randomizationService.GetOrCreateGenerator("Points");
        MessageCache = messageCache;
        ProfilePictureManager = profilePictureManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;

        DiscordClient.ReactionAdded += OnReactionAddedAsync;

        TrophyImage = new MagickImage(MiscResources.trophy, MagickFormat.Png);
    }

    private bool CanIncrement(IMessage message)
    {
        var argPos = 0;

        if (message == null) return false;
        if (!message.Author.IsUser()) return false;
        if (string.IsNullOrEmpty(message.Content)) return false;
        if (message.Content.Length < Configuration.GetValue<int>("MessageMinLength")) return false;
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return false;
        if (message is IUserMessage userMsg && userMsg.ReferencedMessage?.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix) == true) return false;
        return message.Type != MessageType.ApplicationCommand && message.Type != MessageType.ContextMenuCommand;
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
