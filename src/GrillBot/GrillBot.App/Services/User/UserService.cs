using GrillBot.Database.Enums;
using GrillBot.Data.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Extensions;

namespace GrillBot.App.Services.User;

public class UserService
{
    private IConfiguration Configuration { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserService(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration)
    {
        Configuration = configuration;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<bool> CheckUserFlagsAsync(IUser user, UserFlags flags)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user);
        return userEntity?.HaveFlags(flags) ?? false;
    }

    /// <summary>
    /// Creates embed with user informations.
    /// </summary>
    /// <param name="executor">User who called command.</param>
    /// <param name="guild">Guild where user calling command.</param>
    /// <param name="user">The user we want to get information about.</param>
    public async Task<Embed> CreateInfoEmbed(IUser executor, IGuild guild, SocketGuildUser user)
    {
        var userDetailUrl = await CreateWebAdminLink(executor, user);
        var highestRoleWithColor = user.GetHighestRole(true);
        var state = GetUserStateEmote(user, out var userStatus);
        var joinPosition = user.CalculateJoinPosition();
        var roles = user.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Mention).ToList();
        var clients = user.ActiveClients.Select(o => o.ToString()).ToList();

        var embed = new EmbedBuilder()
            .WithAuthor(user.GetFullName(), user.GetUserAvatarUrl(), userDetailUrl)
            .WithCurrentTimestamp()
            .WithFooter(executor)
            .WithTitle("Informace o uživateli")
            .WithColor(highestRoleWithColor?.Color ?? Color.Default)
            .AddField("Stav", $"{state} {userStatus}")
            .AddField("Role", roles.Count > 0 ? string.Join(" ", roles) : "*Uživatel nemá žádné role.*")
            .AddField("Založen", user.CreatedAt.LocalDateTime.ToCzechFormat(), true);

        if (user.JoinedAt != null)
            embed.AddField("Připojen (pořadí)", $"{user.JoinedAt.Value.LocalDateTime.ToCzechFormat()} ({joinPosition}.)", true);

        if (user.PremiumSince != null)
            embed.AddField("Boost od", user.PremiumSince.Value.LocalDateTime.ToCzechFormat(), true);

        if (clients.Count > 0)
            embed.AddField("Aktivní zařízení", string.Join(", ", clients));

        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUserEntity = await repository.GuildUser.FindGuildUserAsync(user, true);
        if (guildUserEntity != null)
        {
            embed
                .AddField("Body", guildUserEntity.Points, true)
                .AddField("Udělené reakce", guildUserEntity.GivenReactions, true)
                .AddField("Obdržené reakce", guildUserEntity.ObtainedReactions, true);
        }

        var messagesCount = await repository.Channel.GetMessagesCountOfUserAsync(user);
        embed.AddField("Počet zpráv", messagesCount, true);

        var unverifyStats = await repository.Unverify.GetUserStatsAsync(user);
        if (unverifyStats.unverify + unverifyStats.selfunverify > 0)
        {
            embed
                .AddField("Počet unverify", unverifyStats.unverify, true)
                .AddField("Počet selfunverify", unverifyStats.selfunverify, true);
        }

        if (guildUserEntity?.UsedInvite != null)
        {
            var invite = guildUserEntity.UsedInvite;
            var isVanity = invite.Code == guild.VanityURLCode;
            var creator = (isVanity ? null : invite.Creator)?.FullName();

            embed.AddField(
                "Použitá pozvánka",
                $"**{invite.Code}**\n{(isVanity ? "Vanity invite" : $"Založil: **{creator}** (**{invite.CreatedAt?.ToCzechFormat()}**)")}"
            );
        }

        var topChannels = await repository.Channel.GetTopChannelsOfUserAsync(user);

        if (topChannels.mostActive != null)
            embed.AddField("Nejaktivnější kanál", $"<#{topChannels.mostActive.ChannelId}> ({topChannels.mostActive.Count})");

        // ReSharper disable once InvertIf
        if (topChannels.lastActive != null)
            embed.AddField("Poslední zpráva", $"<#{topChannels.lastActive.ChannelId}> ({topChannels.lastActive.LastMessageAt.ToCzechFormat()})");

        return embed.Build();
    }

    public async Task<string> CreateWebAdminLink(IUser executor, IUser user)
    {
        if (!await CheckUserFlagsAsync(executor, UserFlags.WebAdmin)) return null;

        var value = Configuration.GetValue<string>("WebAdmin:UserDetailLink");
        return string.Format(value, user.Id);
    }

    public Emote GetUserStateEmote(IUser user, out string userStatus)
    {
        var status = user.GetStatus();
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
