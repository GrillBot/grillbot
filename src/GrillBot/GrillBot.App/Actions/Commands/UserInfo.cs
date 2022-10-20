using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Commands;

public class UserInfo : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IConfiguration Configuration { get; }
    private ITextsManager Texts { get; }

    private bool OverLimit { get; set; }
    private Database.Entity.User ExecutorEntity { get; set; }
    private IEnumerable<IGuildUser> GuildUsers { get; set; }

    public UserInfo(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, ITextsManager texts)
    {
        DatabaseBuilder = databaseBuilder;
        Configuration = configuration;
        Texts = texts;
    }

    public async Task<Embed> ProcessAsync(IGuildUser user)
    {
        GuildUsers = await Context.Guild.GetUsersAsync();

        await using var repository = DatabaseBuilder.CreateRepository();
        ExecutorEntity = await repository.User.FindUserAsync(Context.User, true);
        var entity = await repository.GuildUser.FindGuildUserAsync(user, true, true);

        var builder = Init(user);
        SetAuthor(builder, user);
        SetCommonInfo(builder, user);
        SetGuildInfo(builder, user);
        await SetDatabaseInfoAsync(builder, user, entity, repository);

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
        AddField(builder, "CreatedAt", user.CreatedAt.ToCzechFormat(), true);

        if (user.ActiveClients.Count > 0)
            AddField(builder, "ActiveDevices", string.Join(", ", user.ActiveClients), true);
    }

    private void SetUserState(EmbedBuilder builder, IUser user)
    {
        var status = user.GetStatus();
        var emote = Configuration.GetValue<string>($"Discord:Emotes:{status}");

        AddField(builder, "State", $"{Emote.Parse(emote)} {Texts[$"User/UserStatus/{status}", Locale]}", true);
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
            var joinPosition = GuildUsers.Count(o => o.JoinedAt != null && o.JoinedAt.Value < user.JoinedAt.Value);
            AddField(builder, "JoinedAt", $"{user.JoinedAt.Value.ToCzechFormat()} ({joinPosition})", true);
        }

        if (user.PremiumSince != null)
            AddField(builder, "PremiumSince", user.PremiumSince.Value.ToCzechFormat(), true);
    }

    private async Task SetDatabaseInfoAsync(EmbedBuilder builder, IGuildUser user, Database.Entity.GuildUser entity, GrillBotRepository repository)
    {
        await SetPointsInfoAsync(builder, user, repository);

        if (entity.GivenReactions + entity.ObtainedReactions > 0)
            AddField(builder, "Reactions", $"{entity.GivenReactions} / {entity.ObtainedReactions}", true);

        await SetMessageInfoAsync(builder, user, repository);
        await SetUnverifyInfoAsync(builder, user, repository);
        SetInviteInfo(builder, entity);
        await SetChannelInfoAsync(builder, user, repository);
    }

    private async Task SetPointsInfoAsync(EmbedBuilder builder, IGuildUser user, GrillBotRepository repository)
    {
        if (OverLimit) return;
        var points = await repository.Points.ComputePointsOfUserAsync(Context.Guild.Id, user.Id);
        if (points > 0)
            AddField(builder, "Points", points.ToString(), true);
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
        var (unverifyCount, selfunverifyCount) = await repository.Unverify.GetUserStatsAsync(user);
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
            AddField(builder, "UnverifyInfo", row.Cut(EmbedFieldBuilder.MaxFieldValueLength, true), false);
        }
    }

    private void SetInviteInfo(EmbedBuilder builder, Database.Entity.GuildUser entity)
    {
        if (entity.UsedInvite == null) return;

        var invite = entity.UsedInvite;
        var inviteRow = invite.Code == Context.Guild.VanityURLCode
            ? Texts["User/InfoEmbed/UsedVanityInviteRow", Locale].FormatWith(invite.Code, Texts["User/InfoEmbed/VanityInvite", Locale])
            : Texts["User/InfoEmbed/UsedInviteRow", Locale].FormatWith(invite.Code, invite.Creator!.FullName(), invite.CreatedAt!.Value.ToCzechFormat());
        AddField(builder, "UsedInvite", inviteRow, false);
    }

    private async Task SetChannelInfoAsync(EmbedBuilder builder, IGuildUser user, GrillBotRepository repository)
    {
        if (OverLimit) return;
        var (mostActiveChanel, lastActiveChannel) = await repository.Channel.GetTopChannelStatsOfUserAsync(user);

        if (mostActiveChanel != null)
            AddField(builder, "MostActiveChannel", $"<#{mostActiveChanel.ChannelId}> ({mostActiveChanel.Count})", false);
        if (lastActiveChannel != null)
            AddField(builder, "LastMessageIn", $"<#{lastActiveChannel.ChannelId}> ({lastActiveChannel.Count})", false);
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
