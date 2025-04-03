using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.InviteService;
using GrillBot.Core.Services.InviteService.Models.Request;

namespace GrillBot.App.Actions.Commands;

public class UserInfo : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IConfiguration Configuration { get; }
    private ITextsManager Texts { get; }

    private bool OverLimit { get; set; }
    private Database.Entity.User ExecutorEntity { get; set; } = null!;
    private IEnumerable<IGuildUser> GuildUsers { get; set; } = new List<IGuildUser>();

    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;
    private readonly IServiceClientExecutor<IInviteServiceClient> _inviteServiceClient;

    public UserInfo(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, ITextsManager texts, IServiceClientExecutor<IPointsServiceClient> pointsServiceClient,
        IServiceClientExecutor<IInviteServiceClient> inviteServiceClient)
    {
        DatabaseBuilder = databaseBuilder;
        Configuration = configuration;
        Texts = texts;
        _pointsServiceClient = pointsServiceClient;
        _inviteServiceClient = inviteServiceClient;
    }

    public async Task<Embed> ProcessAsync(IGuildUser user)
    {
        GuildUsers = await Context.Guild.GetUsersAsync();

        using var repository = DatabaseBuilder.CreateRepository();
        ExecutorEntity = (await repository.User.FindUserAsync(Context.User, true))!;
        var userEntity = (await repository.GuildUser.FindGuildUserAsync(user, true, true))!;

        var builder = Init(user);
        SetAuthor(builder, user);
        SetCommonInfo(builder, user);
        SetGuildInfo(builder, user);
        await SetDatabaseInfoAsync(builder, user, userEntity, repository);

        if (!OverLimit || !ExecutorEntity.HaveFlags(UserFlags.WebAdmin))
            return builder.Build();

        builder.Fields.RemoveAt(EmbedBuilder.MaxFieldCount - 1); // Remove last item to add info about over limit fields.
        AddField(builder, "WebAdminDetails", Texts["User/InfoEmbed/WebAdminDetails", Locale], false);
        return builder.Build();
    }

    private EmbedBuilder Init(IGuildUser user)
    {
        return new EmbedBuilder()
            .WithColor(user.GetHighestRole(true)?.Color ?? Color.Default)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .WithTitle(Texts["User/InfoEmbed/Title", Locale]);
    }

    private void SetAuthor(EmbedBuilder builder, IUser user)
    {
        var webAdminLink = !ExecutorEntity.HaveFlags(UserFlags.WebAdmin) ? null : Configuration.GetValue<string>("WebAdmin:UserDetailLink").FormatWith(user.Id);
        builder.WithAuthor(user.GetFullName(), user.GetUserAvatarUrl(), webAdminLink);
    }

    private void SetCommonInfo(EmbedBuilder builder, IUser user)
    {
        SetUserState(builder, user);
        AddField(builder, "CreatedAt", user.CreatedAt.ToTimestampMention(), true);
        SetUserBadges(builder, user);

        if (user.ActiveClients.Count > 0)
            AddField(builder, "ActiveDevices", string.Join(", ", user.ActiveClients), true);
    }

    private void SetUserState(EmbedBuilder builder, IUser user)
    {
        var status = user.GetStatus();
        var emote = Configuration.GetValue<string>($"Discord:Emotes:{status}");

        AddField(builder, "State", $"{Emote.Parse(emote)} {Texts[$"User/UserStatus/{status}", Locale]}", true);
    }

    private void SetUserBadges(EmbedBuilder builder, IUser user)
    {
        if (user.PublicFlags is null || user.PublicFlags == UserProperties.None)
            return;

        var badges = new List<string>();

        if (user.PublicFlags.Value.HasFlag(UserProperties.HypeSquadBravery))
            badges.Add(Configuration.GetValue<string>("Discord:Emotes:HypeSquadBravery")!);
        if (user.PublicFlags.Value.HasFlag(UserProperties.HypeSquadBalance))
            badges.Add(Configuration.GetValue<string>("Discord:Emotes:HypeSquadBalance")!);
        if (user.PublicFlags.Value.HasFlag(UserProperties.HypeSquadBrilliance))
            badges.Add(Configuration.GetValue<string>("Discord:Emotes:HypeSquadBriliance")!);
        if (user.PublicFlags.Value.HasFlag(UserProperties.ActiveDeveloper))
            badges.Add(Configuration.GetValue<string>("Discord:Emotes:ActiveDeveloper")!);
        if (user.PublicFlags.Value.HasFlag(UserProperties.Partner))
            badges.Add(Configuration.GetValue<string>("Discord:Emotes:Partner")!);
        if (user.PublicFlags.Value.HasFlag(UserProperties.EarlySupporter))
            badges.Add(Configuration.GetValue<string>("Discord:Emotes:EarlySupporter")!);

        if (badges.Count > 0)
            AddField(builder, "Badges", string.Join(" ", badges), false);
    }

    private void SetRoles(EmbedBuilder builder, IGuildUser user)
    {
        var roles = user.GetRoles().Select(o => o.Mention).ToList();
        if (roles.Count == 0)
        {
            AddField(builder, "Roles", Texts["User/InfoEmbed/NoRoles", Locale], false);
            return;
        }

        var fieldValue = new StringBuilder();
        foreach (var role in roles)
        {
            if (fieldValue.Length + role.Length + 1 >= EmbedFieldBuilder.MaxFieldValueLength)
            {
                AddField(builder, "Roles", fieldValue.ToString().Trim(), false);
                fieldValue.Clear();
            }

            fieldValue.Append(role).Append(' ');
        }

        if (fieldValue.Length > 0)
            AddField(builder, "Roles", fieldValue.ToString(), false);
    }

    private void SetGuildInfo(EmbedBuilder builder, IGuildUser user)
    {
        SetRoles(builder, user);

        if (user.JoinedAt != null)
        {
            var joinPosition = GuildUsers.Count(o => o.JoinedAt < user.JoinedAt.Value);
            AddField(builder, "JoinedAt", $"{user.JoinedAt.Value.ToTimestampMention()} ({joinPosition})", true);
        }

        if (user.PremiumSince != null)
            AddField(builder, "PremiumSince", user.PremiumSince.Value.ToTimestampMention(), true);
    }

    private async Task SetDatabaseInfoAsync(EmbedBuilder builder, IGuildUser user, Database.Entity.GuildUser userEntity, GrillBotRepository repository)
    {
        await SetPointsInfoAsync(builder, user);

        if (userEntity.GivenReactions + userEntity.ObtainedReactions > 0)
            AddField(builder, "Reactions", $"{userEntity.GivenReactions} / {userEntity.ObtainedReactions}", true);

        await SetMessageInfoAsync(builder, user, repository);
        await SetUnverifyInfoAsync(builder, user, repository);
        await SetUserInviteAsync(builder, userEntity);
        await SetChannelInfoAsync(builder, user, repository);
    }

    private async Task SetPointsInfoAsync(EmbedBuilder builder, IGuildUser user)
    {
        if (OverLimit) return;

        var pointsStatus = await _pointsServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetStatusOfPointsAsync(Context.Guild.Id.ToString(), user.Id.ToString(), cancellationToken));
        if (pointsStatus.YearBack > 0)
            AddField(builder, "Points", pointsStatus.YearBack.ToString(), true);
    }

    private async Task SetMessageInfoAsync(EmbedBuilder builder, IGuildUser user, GrillBotRepository repository)
    {
        if (OverLimit) return;
        var messageCount = await repository.Channel.GetMessagesCountOfUserAsync(user);
        if (messageCount > 0)
            AddField(builder, "MessageCount", messageCount.ToString(), true);
    }

    private async Task SetUnverifyInfoAsync(EmbedBuilder builder, IGuildUser user, GrillBotRepository repository)
    {
        if (OverLimit) return;

        var (unverifyCount, selfunverifyCount) = await repository.Unverify.GetUserStatsAsync(user.GuildId.ToString(), user.Id.ToString());
        if (unverifyCount > 0) AddField(builder, "UnverifyCount", unverifyCount.ToString(), true);
        if (selfunverifyCount > 0) AddField(builder, "SelfUnverifyCount", selfunverifyCount.ToString(), true);

        if (OverLimit) return;
        var unverify = await repository.Unverify.FindUnverifyAsync(Context.Guild.Id, user.Id, true, true);
        if (unverify != null)
        {
            var unverifyLogData = JsonConvert.DeserializeObject<UnverifyLogSet>(unverify.UnverifyLog!.Data)!;
            var unverifyType = unverifyLogData.IsSelfUnverify ? "self" : "";
            var reason = unverifyLogData.IsSelfUnverify ? "" : Texts["User/InfoEmbed/ReasonRow", Locale].FormatWith(unverify.Reason);
            var row = Texts["User/InfoEmbed/UnverifyRow", Locale].FormatWith(unverifyType, unverify.EndAt.ToCzechFormat(), reason);
            AddField(builder, "UnverifyInfo", row.Cut(EmbedFieldBuilder.MaxFieldValueLength, true)!, false);
        }
    }

    private async Task SetUserInviteAsync(EmbedBuilder builder, Database.Entity.GuildUser entity)
    {
        if (OverLimit)
            return;

        var request = new UserInviteUseListRequest
        {
            UserId = entity.UserId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            Sort =
            {
                Descending = true
            }
        };

        var invites = await _inviteServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetUserInviteUsesAsync(request, cancellationToken));
        var invite = invites.Data.FirstOrDefault(o => o.GuildId == entity.GuildId);

        if (invite is null)
            return;

        if (invite.Code == Context.Guild.VanityURLCode)
        {
            AddField(
                builder,
                "UsedInvite",
                Texts["User/InfoEmbed/UsedVanityInviteRow", Locale].FormatWith(invite.Code, Texts["User/InfoEmbed/VanityInvite", Locale]),
                false
            );
        }
        else
        {
            var inviteInfoRequest = new InviteListRequest
            {
                Code = invite.Code,
                GuildId = Context.Guild.Id.ToString(),
                Pagination =
                {
                    Page = 0,
                    PageSize = 1
                }
            };

            var inviteInfo = await _inviteServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetUsedInvitesAsync(inviteInfoRequest, cancellationToken));
            var creator = await Context.Guild.GetUserAsync(inviteInfo.Data[0].CreatorId.ToUlong());
            var createdAt = inviteInfo.Data[0].CreatedAt!.Value.ToLocalTime().ToCzechFormat();

            AddField(
                builder,
                "UsedInvite",
                Texts["User/InfoEmbed/UsedInviteRow", Locale].FormatWith(invite.Code, creator.GetFullName(), createdAt),
                false
            );
        }
    }

    private async Task SetChannelInfoAsync(EmbedBuilder builder, IGuildUser user, GrillBotRepository repository)
    {
        if (OverLimit)
            return;
        var (lastActiveChannel, mostActiveChanel) = await repository.Channel.GetTopChannelStatsOfUserAsync(user);

        if (mostActiveChanel is not null)
            AddField(builder, "MostActiveChannel", $"{mostActiveChanel.Channel.GetHyperlink()} ({mostActiveChanel.Count})", false);
        if (lastActiveChannel is not null)
            AddField(builder, "LastMessageIn", $"{lastActiveChannel.Channel.GetHyperlink()} ({lastActiveChannel.Count})", false);
    }

    private void AddField(EmbedBuilder builder, string fieldId, string value, bool inline)
    {
        if (builder.Fields.Count == EmbedBuilder.MaxFieldCount)
        {
            OverLimit = true;
            return;
        }

        builder.AddField(Texts[$"User/InfoEmbed/Fields/{fieldId}", Locale], value, inline);
    }
}
