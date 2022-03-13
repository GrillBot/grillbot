using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.IO;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Resources.Misc;
using GrillBot.Database.Entity;
using ImageMagick;

namespace GrillBot.App.Services.User;

[Initializable]
public class PointsService : ServiceBase
{
    private string CommandPrefix { get; }
    private IConfiguration Configuration { get; }
    private Random Random { get; }
    private FileStorageFactory FileStorageFactory { get; }
    private MessageCache.MessageCache MessageCache { get; }

    private MagickImage TrophyImage { get; }

    public PointsService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
        FileStorageFactory fileStorageFactory, MessageCache.MessageCache messageCache, RandomizationService randomizationService) : base(client, dbFactory)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        Configuration = configuration.GetSection("Points");
        Random = randomizationService.GetOrCreateGenerator("Points");
        FileStorageFactory = fileStorageFactory;
        MessageCache = messageCache;

        DiscordClient.MessageReceived += (message) => message.TryLoadMessage(out SocketUserMessage msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
        DiscordClient.ReactionAdded += OnReactionAddedAsync;

        TrophyImage = new MagickImage(MiscResources.trophy, MagickFormat.Png);
    }

    private async Task OnMessageReceivedAsync(SocketUserMessage message)
    {
        if (!CanIncrement(message)) return;
        if (message.Channel is not SocketTextChannel textChannel) return;

        var guildId = textChannel.Guild.Id.ToString();
        var guildUserEntity = message.Author as IGuildUser ?? textChannel.Guild.GetUser(message.Author.Id);
        var userId = guildUserEntity.Id.ToString();

        using var context = DbFactory.Create();

        await context.InitGuildAsync(textChannel.Guild, CancellationToken.None);
        await context.InitUserAsync(message.Author, CancellationToken.None);

        var guildUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

        if (guildUser == null)
        {
            guildUser = GuildUser.FromDiscord(textChannel.Guild, guildUserEntity);
            await context.AddAsync(guildUser);
        }

        IncrementPoints(
            guildUser,
            user => user.LastPointsMessageIncrement,
            Configuration.GetSection("Range:Message"),
            Configuration.GetValue<int>("Cooldown:Message"),
            user => user.LastPointsMessageIncrement = DateTime.Now
        );

        await context.SaveChangesAsync();
    }

    private bool CanIncrement(IUserMessage message)
    {
        int argPos = 0;

        if (message == null) return false;
        if (string.IsNullOrEmpty(message.Content)) return false;
        if (message.Content.Length < Configuration.GetValue<int>("MessageMinLength")) return false;
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix)) return false;

        return true;
    }

    private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!channel.HasValue || channel.Value is not SocketTextChannel textChannel) return; // Only guilds
        if (reaction.Emote is not Emoji && !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return; // Only local emotes.

        var user = (reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId)) as IGuildUser;
        if (user?.IsUser() != true) return;

        int argPos = 0;
        var msg = message.HasValue ? message.Value : (await MessageCache.GetMessageAsync(textChannel, message.Id)) as IUserMessage;
        if (!CanIncrement(msg)) return;
        if (msg.ReferencedMessage?.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix) == true) return;

        var guildId = textChannel.Guild.Id.ToString();
        var userId = reaction.UserId.ToString();

        using var context = DbFactory.Create();

        await context.InitGuildAsync(textChannel.Guild, CancellationToken.None);
        await context.InitUserAsync(reaction.User.Value, CancellationToken.None);

        var guildUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

        if (guildUser == null)
        {
            guildUser = GuildUser.FromDiscord(textChannel.Guild, user);
            await context.AddAsync(guildUser);
        }

        IncrementPoints(
            guildUser,
            guildUser => guildUser.LastPointsReactionIncrement,
            Configuration.GetSection("Range:Reaction"),
            Configuration.GetValue<int>("Cooldown:Reaction"),
            guildUser => guildUser.LastPointsReactionIncrement = DateTime.Now
        );

        await context.SaveChangesAsync();
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
        using var dbContext = DbFactory.Create();

        var guildUser = await dbContext.GuildUsers.AsNoTracking()
            .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());

        if (guildUser == null)
            throw new NotFoundException($"{user.GetDisplayName()} ještě neprojevil na serveru žádnou aktivitu.");

        const int height = 340;
        const int width = 1000;
        const int border = 25;
        const double nicknameFontSize = 80;
        const int profilePictureSize = 250;
        const string fontName = "Open Sans";

        var position = await CalculatePointsPositionAsync(dbContext, guild, user);
        var nickname = user.GetDisplayName(false);

        using var profilePicture = await GetProfilePictureAsync(user);
        var cuttedNickname = nickname.CutToImageWidth(width - (border * 4) - profilePictureSize, fontName, nicknameFontSize);

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

        var pointsInfo = $"{position}. místo\n{FormatHelper.FormatPointsToCzech(guildUser.Points)}";
        drawable
            .Font("Arial")
            .FontPointSize(60)
            .Text(pointsInfoX, 210, pointsInfo);

        drawable.Draw(image);

        var tmpFile = new TemporaryFile("png");
        await image.WriteAsync(tmpFile.Path, MagickFormat.Png);
        return tmpFile;
    }

    private static async Task<int> CalculatePointsPositionAsync(GrillBotContext context, IGuild guild, IUser user)
    {
        var guildId = guild.Id.ToString();

        var query = context.GuildUsers.AsQueryable()
            .AsNoTracking()
            .Where(o => o.GuildId == guildId && o.UserId == user.Id.ToString())
            .Select(o => o.Points)
            .SelectMany(pts => context.GuildUsers.AsQueryable().Where(o => o.GuildId == guildId && o.Points > pts));

        return (await query.CountAsync()) + 1;
    }

    private async Task<MagickImage> GetProfilePictureAsync(IUser user)
    {
        var cache = FileStorageFactory.CreateCache();
        var filename = $"{user.Id}_{user.AvatarId ?? user.Discriminator}_256.{(user.HaveAnimatedAvatar() ? "gif" : "png")}";
        var fileinfo = await cache.GetProfilePictureInfoAsync(filename);

        if (!fileinfo.Exists)
        {
            var profilePicture = await user.DownloadAvatarAsync(size: 256);
            await cache.StoreProfilePictureAsync(filename, profilePicture);
        }

        return new MagickImage(fileinfo.FullName);
    }

    public async Task IncrementPointsAsync(SocketGuild guild, SocketGuildUser toUser, int amount)
    {
        var guildId = guild.Id.ToString();
        var userId = toUser.Id.ToString();

        using var context = DbFactory.Create();

        await context.InitGuildAsync(guild, CancellationToken.None);
        await context.InitUserAsync(toUser, CancellationToken.None);

        var guildUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

        if (guildUser == null)
        {
            guildUser = GuildUser.FromDiscord(guild, toUser);
            await context.AddAsync(guildUser);
        }

        guildUser.Points += amount;
        await context.SaveChangesAsync();
    }

    public async Task TransferPointsAsync(SocketGuild guild, SocketUser fromUser, SocketGuildUser toUser, long amount)
    {
        if (fromUser == toUser)
            throw new InvalidOperationException("Nelze převést body mezi stejnými účty.");

        if (!fromUser.IsUser())
            throw new InvalidOperationException($"Nelze převést body od `{fromUser.GetDisplayName()}`, protože se nejedná o běžného uživatele.");

        if (!toUser.IsUser())
            throw new InvalidOperationException($"Nelze převést body uživateli `{toUser.GetDisplayName()}`, protože se nejedná o běžného uživatele.");

        var guildId = guild.Id.ToString();
        var fromUserId = fromUser.Id.ToString();
        var toUserId = toUser.Id.ToString();

        using var context = DbFactory.Create();

        var fromGuildUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == fromUserId);

        if (fromGuildUser == null)
            throw new InvalidOperationException($"Nelze převést body od uživatele `{fromUser.GetDisplayName()}`, protože žádné body ještě nemá.");

        if (fromGuildUser.Points < amount)
            throw new InvalidOperationException($"Nelze převést body od uživatele `{fromUser.GetDisplayName()}`, protože jich nemá dostatek.");

        await context.InitGuildAsync(guild, CancellationToken.None);
        await context.InitUserAsync(toUser, CancellationToken.None);

        var toGuildUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == toUserId);

        if (toGuildUser == null)
        {
            toGuildUser = GuildUser.FromDiscord(guild, toUser);
            await context.AddAsync(toGuildUser);
        }

        toGuildUser.Points += amount;
        fromGuildUser.Points -= amount;
        await context.SaveChangesAsync();
    }
}
