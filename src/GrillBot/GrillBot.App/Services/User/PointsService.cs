using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Resources.Misc;
using GrillBot.Database.Entity;
using ImageMagick;

namespace GrillBot.App.Services.User;

[Initializable]
public class PointsService
{
    private string CommandPrefix { get; }
    private IConfiguration Configuration { get; }
    private Random Random { get; }
    private MessageCacheManager MessageCache { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private MagickImage TrophyImage { get; }

    public PointsService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration,
        MessageCacheManager messageCache, RandomizationService randomizationService,
        ProfilePictureManager profilePictureManager)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        Configuration = configuration.GetSection("Points");
        Random = randomizationService.GetOrCreateGenerator("Points");
        MessageCache = messageCache;
        ProfilePictureManager = profilePictureManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.MessageReceived += (message) => message.TryLoadMessage(out SocketUserMessage msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
        DiscordClient.ReactionAdded += OnReactionAddedAsync;

        TrophyImage = new MagickImage(MiscResources.trophy, MagickFormat.Png);
    }

    private async Task OnMessageReceivedAsync(SocketUserMessage message)
    {
        if (!CanIncrement(message)) return;
        if (message.Channel is not SocketTextChannel textChannel) return;

        var guildUserEntity = message.Author as IGuildUser ?? textChannel.Guild.GetUser(message.Author.Id);

        await using var repository = DatabaseBuilder.CreateRepository();
        var guildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(guildUserEntity);

        IncrementPoints(
            guildUser,
            user => user.LastPointsMessageIncrement,
            Configuration.GetSection("Range:Message"),
            Configuration.GetValue<int>("Cooldown:Message"),
            user => user.LastPointsMessageIncrement = DateTime.Now
        );

        await repository.CommitAsync();
    }

    private bool CanIncrement(IUserMessage message)
    {
        var argPos = 0;

        if (message == null) return false;
        if (string.IsNullOrEmpty(message.Content)) return false;
        if (message.Content.Length < Configuration.GetValue<int>("MessageMinLength")) return false;
        return !message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix);
    }

    private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!channel.HasValue || channel.Value is not SocketTextChannel textChannel) return; // Only guilds
        if (reaction.Emote is not Emoji && !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return; // Only local emotes.

        var user = (reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId)) as IGuildUser;
        if (user?.IsUser() != true) return;

        var argPos = 0;
        var msg = message.HasValue ? message.Value : (await MessageCache.GetAsync(message.Id, textChannel)) as IUserMessage;
        if (!CanIncrement(msg)) return;
        if (msg!.ReferencedMessage?.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix) == true) return;

        await using var repository = DatabaseBuilder.CreateRepository();
        var guildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(user);

        IncrementPoints(
            guildUser,
            gu => gu.LastPointsReactionIncrement,
            Configuration.GetSection("Range:Reaction"),
            Configuration.GetValue<int>("Cooldown:Reaction"),
            gu => gu.LastPointsReactionIncrement = DateTime.Now
        );

        await repository.CommitAsync();
    }

    private void IncrementPoints(GuildUser user, Func<GuildUser, DateTime?> lastIncrementSelector, IConfiguration range, int cooldown, Action<GuildUser> lastIncrementReset)
    {
        var lastIncrement = lastIncrementSelector(user);
        if (lastIncrement.HasValue && lastIncrement.Value.AddSeconds(cooldown) > DateTime.Now)
            return;

        user.Points += Random.Next(range.GetValue<int>("From"), range.GetValue<int>("To"));
        lastIncrementReset(user);
    }

    public async Task<TemporaryFile> GetPointsOfUserImageAsync(IGuild guild, IUser user)
    {
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);

        await using var repository = DatabaseBuilder.CreateRepository();
        var guildUserEntity = await repository.GuildUser.FindGuildUserAsync(guildUser, true);

        if (guildUserEntity == null)
            throw new NotFoundException($"{user.GetDisplayName()} ještě neprojevil na serveru žádnou aktivitu.");

        const int height = 340;
        const int width = 1000;
        const int border = 25;
        const double nicknameFontSize = 80;
        const int profilePictureSize = 250;
        const string fontName = "Open Sans";

        var position = await repository.GuildUser.CalculatePointsPositionAsync(guildUser);
        var nickname = user.GetDisplayName(false);

        using var profilePicture = await GetProfilePictureAsync(user);
        var cuttedNickname = nickname.CutToImageWidth(width - border * 4 - profilePictureSize, fontName, nicknameFontSize);

        var dominantColor = profilePicture.GetDominantColor();
        var textBackground = dominantColor.CreateDarkerBackgroundColor();

        using var image = new MagickImage(dominantColor, width, height);

        var drawable = new Drawables()
            .StrokeAntialias(true)
            .TextAntialias(true)
            .FillColor(textBackground)
            .RoundRectangle(border, border, width - border, height - border, 20, 20)
            .TextAlignment(TextAlignment.Left)
            .Font(fontName)
            .FontPointSize(nicknameFontSize)
            .FillColor(MagickColors.White)
            .Text(320, 130, cuttedNickname);

        // Profile picture operations
        profilePicture.Resize(profilePictureSize, profilePictureSize);
        profilePicture.RoundImage();
        drawable.Composite(border * 2, border * 2, CompositeOperator.Over, profilePicture);

        double pointsInfoX = 320;
        if (position == 1)
        {
            drawable.Composite(pointsInfoX, 170, CompositeOperator.Over, TrophyImage);
            pointsInfoX += TrophyImage.Width + 10;
        }

        var pointsInfo = $"{position}. místo\n{FormatHelper.FormatPointsToCzech(guildUserEntity.Points)}";
        drawable
            .Font("Arial")
            .FontPointSize(60)
            .Text(pointsInfoX, 210, pointsInfo);

        drawable.Draw(image);

        var tmpFile = new TemporaryFile("png");
        await image.WriteAsync(tmpFile.Path, MagickFormat.Png);
        return tmpFile;
    }

    private async Task<MagickImage> GetProfilePictureAsync(IUser user)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, 256);
        return new MagickImage(profilePicture.Data);
    }

    public async Task IncrementPointsAsync(IGuildUser toUser, int amount)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateRepositoryAsync(toUser.Guild);
        await repository.User.GetOrCreateUserAsync(toUser);
        var guildUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        guildUserEntity.Points += amount;
        await repository.CommitAsync();
    }

    public async Task TransferPointsAsync(IGuildUser fromUser, IGuildUser toUser, long amount)
    {
        if (fromUser.Id == toUser.Id)
            throw new InvalidOperationException("Nelze převést body mezi stejnými účty.");

        if (!fromUser.IsUser())
            throw new InvalidOperationException($"Nelze převést body od `{fromUser.GetDisplayName()}`, protože se nejedná o běžného uživatele.");

        if (!toUser.IsUser())
            throw new InvalidOperationException($"Nelze převést body uživateli `{toUser.GetDisplayName()}`, protože se nejedná o běžného uživatele.");

        await using var repository = DatabaseBuilder.CreateRepository();

        var fromGuildUser = await repository.GuildUser.FindGuildUserAsync(fromUser);
        if (fromGuildUser == null)
            throw new InvalidOperationException($"Nelze převést body od uživatele `{fromUser.GetDisplayName()}`, protože žádné body ještě nemá.");

        if (fromGuildUser.Points < amount)
            throw new InvalidOperationException($"Nelze převést body od uživatele `{fromUser.GetDisplayName()}`, protože jich nemá dostatek.");

        await repository.User.GetOrCreateUserAsync(toUser);
        await repository.Guild.GetOrCreateRepositoryAsync(toUser.Guild);
        var toGuildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(toUser);

        toGuildUser.Points += amount;
        fromGuildUser.Points -= amount;
        await repository.CommitAsync();
    }
}
