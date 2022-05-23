using GrillBot.Database.Enums;
using GrillBot.Data.Extensions;
using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Services.User;

public class UserService : ServiceBase
{
    private IConfiguration Configuration { get; }

    public UserService(GrillBotContextFactory dbFactory, IConfiguration configuration, DiscordSocketClient discordClient) : base(discordClient, dbFactory)
    {
        Configuration = configuration;
    }

    public async Task<bool> IsUserBotAdminAsync(IUser user)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

        return dbUser?.HaveFlags(UserFlags.BotAdmin) ?? false;
    }

    public async Task<bool> WebAdminAllowedAsync(IUser user)
    {
        using var context = DbFactory.Create();

        var dbUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(o => o.Id == user.Id.ToString());
        return dbUser?.HaveFlags(UserFlags.WebAdmin) ?? false;
    }

    /// <summary>
    /// Creates embed with user informations.
    /// </summary>
    /// <param name="executor">User who called command.</param>
    /// <param name="guild">Guild where user calling command.</param>
    /// <param name="user">The user we want to get information about.</param>
    public async Task<Embed> CreateInfoEmbed(IUser executor, IGuild guild, SocketGuildUser user)
    {
        using var dbContext = DbFactory.Create();

        var userDetailUrl = await CreateWebAdminLink(executor, user);
        var highestRoleWithColor = user.GetHighestRole(true);
        var state = GetUserStateEmote(user, out var userStatus);
        var joinPosition = user.CalculateJoinPosition();
        var roles = user.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Mention);
        var clients = user.ActiveClients.Select(o => o.ToString());

        var embed = new EmbedBuilder()
            .WithAuthor(user.GetFullName(), user.GetUserAvatarUrl(), userDetailUrl)
            .WithCurrentTimestamp()
            .WithFooter(executor)
            .WithTitle("Informace o uživateli")
            .WithColor(highestRoleWithColor?.Color ?? Color.Default)
            .AddField("Stav", $"{state} {userStatus}", false)
            .AddField("Role", roles.Any() ? string.Join(" ", roles) : "*Uživatel nemá žádné role.*", false)
            .AddField("Založen", user.CreatedAt.LocalDateTime.ToCzechFormat(), true)
            .AddField("Připojen (pořadí)", $"{user.JoinedAt.Value.LocalDateTime.ToCzechFormat()} ({joinPosition}.)", true);

        if (user.PremiumSince != null)
            embed.AddField("Boost od", user.PremiumSince.Value.LocalDateTime.ToCzechFormat(), true);

        if (clients.Any())
            embed.AddField("Aktivní zařízení", string.Join(", ", clients), false);

        var userStateQueryBase = dbContext.GuildUsers.AsNoTracking()
            .Where(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());

        var baseData = await userStateQueryBase.Include(o => o.UsedInvite)
            .Select(o => new { o.Points, o.GivenReactions, o.ObtainedReactions, o.UsedInvite }).FirstOrDefaultAsync();
        if (baseData != null)
        {
            embed.AddField("Body", baseData.Points, true)
                .AddField("Udělené reakce", baseData?.GivenReactions, true)
                .AddField("Obdržené reakce", baseData?.ObtainedReactions, true);
        }

        var messagesCount = await dbContext.UserChannels.AsNoTracking()
            .Where(o =>
                o.Count > 0 &&
                o.GuildId == guild.Id.ToString() &&
                o.UserId == user.Id.ToString() &&
                (o.Channel.Flags & (long)ChannelFlags.StatsHidden) == 0
            ).SumAsync(o => o.Count);
        embed.AddField("Počet zpráv", messagesCount, true);

        var unverifyStatsQuery = dbContext.UnverifyLogs.AsQueryable().AsNoTracking()
            .Where(o => o.ToUserId == user.Id.ToString() && o.GuildId == guild.Id.ToString());

        var unverifyCount = await unverifyStatsQuery.CountAsync(o => o.Operation == UnverifyOperation.Unverify);
        var selfUnverifyCount = await unverifyStatsQuery.CountAsync(o => o.Operation == UnverifyOperation.Selfunverify);

        if (unverifyCount + selfUnverifyCount > 0)
        {
            embed.AddField("Počet unverify", unverifyCount, true)
                .AddField("Počet selfunverify", selfUnverifyCount, true);
        }

        if (baseData?.UsedInvite != null)
        {
            var invite = baseData.UsedInvite;
            bool isVanity = invite.Code == guild.VanityURLCode;
            var creator = isVanity ? null : await DiscordClient.FindUserAsync(invite.CreatorId.ToUlong());

            embed.AddField(
                "Použitá pozvánka",
                $"**{invite.Code}**\n{(isVanity ? "Vanity invite" : $"Založil: **{creator?.GetFullName()}** (**{invite.CreatedAt?.ToCzechFormat()}**)")}",
                false
            );
        }

        var channelActivityQuery = userStateQueryBase.Select(o => new
        {
            MostActive = o.User.Channels
                .Where(x =>
                    x.GuildId == o.GuildId &&
                    (x.Channel.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    x.Channel.ChannelType != ChannelType.PrivateThread &&
                    x.Channel.ChannelType != ChannelType.PublicThread &&
                    x.Count > 0
                )
                .OrderByDescending(o => o.Count)
                .Select(o => new { o.ChannelId, o.Count })
                .FirstOrDefault(),
            LastMessage = o.User.Channels
                .Where(x =>
                    x.GuildId == o.GuildId &&
                    (x.Channel.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    x.Count > 0
                )
                .OrderByDescending(o => o.LastMessageAt)
                .Select(o => new { o.ChannelId, o.LastMessageAt })
                .FirstOrDefault()
        });

        var channelActivity = await channelActivityQuery.FirstOrDefaultAsync();
        if (channelActivity != null)
        {
            if (channelActivity.MostActive != null)
                embed.AddField("Nejaktivnější kanál", $"<#{channelActivity.MostActive.ChannelId}> ({channelActivity.MostActive.Count})", false);

            if (channelActivity.LastMessage != null)
                embed.AddField("Poslední zpráva", $"<#{channelActivity.LastMessage.ChannelId}> ({channelActivity.LastMessage.LastMessageAt.ToCzechFormat()})", false);
        }

        return embed.Build();
    }

    public async Task<string> CreateWebAdminLink(IUser executor, IUser user)
    {
        var isBotAdmin = await WebAdminAllowedAsync(executor);
        if (!isBotAdmin) return null;

        var value = Configuration.GetValue<string>("WebAdmin:UserDetailLink");
        return string.Format(value, user.Id);
    }

    public Emote GetUserStateEmote(IUser user, out string userStatus)
    {
        var status = user.Status;
        if (status == UserStatus.AFK) status = UserStatus.Idle;
        else if (status == UserStatus.Invisible) status = UserStatus.Offline;

        userStatus = status switch
        {
            UserStatus.DoNotDisturb => "Nerušit",
            UserStatus.Idle => "Nepřítomen",
            _ => status.ToString(),
        };

        var emoteData = Configuration.GetValue<string>($"Discord:Emotes:{status}");
        return Emote.Parse(emoteData);
    }
}
