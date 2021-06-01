using Discord;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.IO;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using SysDraw = System.Drawing;

namespace GrillBot.App.Services
{
    public class PointsService : ServiceBase
    {
        private GrillBotContextFactory DbFactory { get; }
        private string CommandPrefix { get; }
        private IConfiguration Configuration { get; }
        private Random Random { get; }
        private FileStorageFactory FileStorageFactory { get; }

        private Font PositionFont { get; }
        private Font NicknameFont { get; }
        private Font TitleTextFont { get; }
        private SolidBrush WhiteBrush { get; }
        private SolidBrush LightGrayBrush { get; }

        public PointsService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
            FileStorageFactory fileStorageFactory) : base(client)
        {
            DbFactory = dbFactory;
            CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
            Configuration = configuration.GetSection("Points");
            Random = new Random();
            FileStorageFactory = fileStorageFactory;

            DiscordClient.MessageReceived += (message) => message.TryLoadMessage(out SocketUserMessage msg) ? OnMessageReceivedAsync(msg) : Task.CompletedTask;
            DiscordClient.ReactionAdded += OnReactionAddedAsync;

            PositionFont = new Font("Comic Sans MS", 45F);
            NicknameFont = new Font("Comic Sans MS", 40F);
            TitleTextFont = new Font("Comic Sans MS", 20F);
            WhiteBrush = new SolidBrush(SysDraw.Color.White);
            LightGrayBrush = new SolidBrush(SysDraw.Color.LightGray);
        }

        private async Task OnMessageReceivedAsync(SocketUserMessage message)
        {
            if (!CanIncrement(message)) return;
            if (message.Channel is not SocketTextChannel textChannel) return;

            var guildId = textChannel.Guild.Id.ToString();
            var userId = message.Author.Id.ToString();

            using var context = DbFactory.Create();

            if (!await context.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId))
                await context.AddAsync(new Guild() { Id = guildId });

            if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId))
                await context.Users.AddAsync(new User() { Id = userId });

            var guildUser = await context.GuildUsers.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

            if (guildUser == null)
            {
                guildUser = new GuildUser()
                {
                    GuildId = guildId,
                    UserId = userId
                };

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

        private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (channel is not SocketTextChannel textChannel) return; // Only guilds
            if (reaction.Emote is not Emoji && !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return; // Only local emotes.

            await textChannel.Guild.DownloadUsersAsync();
            var user = reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId);
            if (user?.IsUser() != true) return;

            int argPos = 0;
            var msg = await message.GetOrDownloadAsync();
            if (!CanIncrement(msg)) return;
            if (msg.ReferencedMessage?.IsCommand(ref argPos, DiscordClient.CurrentUser, CommandPrefix) == true) return;

            var guildId = textChannel.Guild.Id.ToString();
            var userId = reaction.UserId.ToString();

            using var context = DbFactory.Create();

            if (!await context.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId))
                await context.AddAsync(new Guild() { Id = guildId });

            if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId))
                await context.Users.AddAsync(new User() { Id = userId });

            var guildUser = await context.GuildUsers.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);

            if (guildUser == null)
            {
                guildUser = new GuildUser()
                {
                    GuildId = guildId,
                    UserId = userId
                };

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

        public async Task<TemporaryFile> GetPointsOfUserImageAsync(SocketGuild guild, SocketUser user)
        {
            using var dbContext = DbFactory.Create();

            var guildUser = await dbContext.GuildUsers.AsQueryable()
                .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());

            if (guildUser == null)
                throw new NotFoundException($"{user.GetDisplayName()} ještě neprojevil na serveru žádnou aktivitu.");

            var position = await CalculatePointsPositionAsync(dbContext, guild, user);
            var nickname = user.GetDisplayName().Cut(25, true);

            using var bitmap = new Bitmap(1000, 300);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsHelpers.CreateRectangle(graphics, new Rectangle(new Point(0, 0), bitmap.Size), SysDraw.Color.FromArgb(35, 39, 42), 15);
            GraphicsHelpers.CreateRectangle(graphics, new Rectangle(50, 50, 900, 200), SysDraw.Color.FromArgb(100, 0, 0, 0), 15);

            // Profile picture
            using var profilePicture = await GetProfilePictureAsync(user);
            using var roundedProfilePicture = profilePicture.RoundImage();
            graphics.DrawImage(roundedProfilePicture, 70, 70, 160, 160);

            var positionTextSize = graphics.MeasureString($"#{position}", PositionFont);
            var positionTitleTextSize = graphics.MeasureString("POZICE", TitleTextFont);

            graphics.DrawString("BODY", TitleTextFont, WhiteBrush, new PointF(250, 180));
            graphics.DrawString("POZICE", TitleTextFont, WhiteBrush, new PointF(900 - positionTextSize.Width - positionTitleTextSize.Width, 180));

            graphics.DrawString(guildUser.Points.ToString(), PositionFont, LightGrayBrush, new PointF(340, 150));
            graphics.DrawString($"#{position}", PositionFont, WhiteBrush, new PointF(910 - positionTextSize.Width, 150));
            graphics.DrawString(nickname, NicknameFont, WhiteBrush, new PointF(250, 60));

            var tmpFile = new TemporaryFile("png");
            bitmap.Save(tmpFile.Path, SysDraw.Imaging.ImageFormat.Png);

            return tmpFile;
        }

        private static async Task<int> CalculatePointsPositionAsync(GrillBotContext context, SocketGuild guild, SocketUser user)
        {
            var guildId = guild.Id.ToString();

            var query = context.GuildUsers.AsQueryable()
                .Where(o => o.GuildId == guildId && o.UserId == user.Id.ToString())
                .Select(o => o.Points)
                .SelectMany(pts => context.GuildUsers.AsQueryable().Where(o => o.GuildId == guildId && o.Points > pts));

            return (await query.CountAsync()) + 1;
        }

        private async Task<SysDraw.Image> GetProfilePictureAsync(IUser user)
        {
            var cache = FileStorageFactory.CreateCache();
            var filename = $"{user.Id}_{user.AvatarId ?? user.Discriminator}_128.{(user.HaveAnimatedAvatar() ? "gif" : "png")}";
            var fileinfo = await cache.GetProfilePictureInfoAsync(filename);

            if (!fileinfo.Exists)
            {
                var profilePicture = await user.DownloadAvatarAsync(size: 128);
                await cache.StoreProfilePictureAsync(filename, profilePicture);
            }

            return SysDraw.Image.FromFile(fileinfo.FullName);
        }
    }
}
