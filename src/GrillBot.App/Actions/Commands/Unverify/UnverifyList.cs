using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Unverify;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Commands.Unverify;

public class UnverifyList : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }

    public UnverifyList(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, FormatHelper formatHelper)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        FormatHelper = formatHelper;
    }

    public async Task<(Embed embed, MessageComponent paginationComponent)> ProcessAsync(int page)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var unverify = await repository.Unverify.FindUnverifyPageAsync(Context.Guild, page);
        if (unverify == null)
            throw new NotFoundException(Texts["Unverify/ListEmbed/NoUnverify", Locale]);

        var user = await Context.Guild.GetUserAsync(unverify.UserId.ToUlong());
        var profile = UnverifyProfileGenerator.Reconstruct(unverify, user, Context.Guild);
        var hiddenChannels = await repository.Channel.GetAllChannelsAsync(new List<string> { Context.Guild.Id.ToString() }, true, true, ChannelFlag.StatsHidden);
        var hiddenChannelIds = hiddenChannels.Select(o => o.ChannelId.ToUlong()).ToHashSet();

        profile.ChannelsToRemove = profile.ChannelsToRemove.FindAll(o => !hiddenChannelIds.Contains(o.ChannelId));
        profile.ChannelsToKeep = profile.ChannelsToKeep.FindAll(o => !hiddenChannelIds.Contains(o.ChannelId));

        var embed = await CreateEmbedAsync(profile, page);
        var paginationComponents = await CreatePaginationComponentsAsync(page);
        return (embed, paginationComponents);
    }

    public async Task<int> ComputeCountOfUnverifies()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Unverify.GetUnverifyCountsAsync(Context.Guild);
    }

    private async Task<Embed> CreateEmbedAsync(UnverifyUserProfile profile, int page)
    {
        var color = profile.RolesToKeep.Concat(profile.RolesToRemove)
            .Where(o => o.Color != Color.Default)
            .OrderByDescending(o => o.Position)
            .Select(o => o.Color)
            .FirstOrDefault();

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithAuthor(profile.Destination)
            .WithMetadata(new UnverifyListMetadata { Page = page })
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithTitle(Texts["Unverify/ListEmbed/Title", Locale]);

        await foreach (var field in CreateEmbedFieldsAsync(profile))
            embed.AddField(field);
        return embed.Build();
    }

    private async IAsyncEnumerable<EmbedFieldBuilder> CreateEmbedFieldsAsync(UnverifyUserProfile profile)
    {
        yield return CreateField("StartAt", profile.Start.ToCzechFormat(), true);
        yield return CreateField("EndAt", profile.End.ToCzechFormat(), true);
        yield return CreateField("EndFor", FormatEndDate(profile.End), true);
        yield return CreateField("Selfunverify", FormatHelper.FormatBoolean("Unverify/ListEmbed/Boolean", Locale, profile.IsSelfUnverify), true);

        if (!string.IsNullOrEmpty(profile.Reason))
            yield return CreateField("Reason", profile.Reason, false);

        foreach (var roleGroupField in CreateRolesList(profile.RolesToKeep, true)) yield return roleGroupField;
        foreach (var roleGroupField in CreateRolesList(profile.RolesToRemove, false)) yield return roleGroupField;
        await foreach (var channelGroupField in CreateChannelsListAsync(profile.ChannelsToKeep, true)) yield return channelGroupField;
        await foreach (var channelGroupField in CreateChannelsListAsync(profile.ChannelsToRemove, false)) yield return channelGroupField;
    }

    private string FormatEndDate(DateTime endAt)
    {
        var sign = endAt <= DateTime.Now ? "-" : "";
        return sign + (endAt - DateTime.Now).Humanize(culture: Texts.GetCulture(Locale));
    }

    private EmbedFieldBuilder CreateField(string fieldId, string value, bool inline)
        => new EmbedFieldBuilder().WithName(Texts[$"Unverify/ListEmbed/Fields/{fieldId}", Locale]).WithValue(value).WithIsInline(inline);

    private IEnumerable<EmbedFieldBuilder> CreateRolesList(IReadOnlyCollection<IRole> roles, bool isKeep)
    {
        if (roles.Count == 0) yield break;
        var fieldId = isKeep ? "RetainedRoles" : "RemovedRoles";

        var roleMentions = new StringBuilder();
        foreach (var role in roles.Select(o => o.Mention))
        {
            if (roleMentions.Length + role.Length + 1 > EmbedFieldBuilder.MaxFieldValueLength)
            {
                var mentionsValue = roleMentions.ToString();
                roleMentions.Clear();
                yield return CreateField(fieldId, mentionsValue, false);
            }

            roleMentions.Append(role).Append(' ');
        }

        if (roleMentions.Length > 0)
            yield return CreateField(fieldId, roleMentions.ToString(), false);
    }

    private async IAsyncEnumerable<EmbedFieldBuilder> CreateChannelsListAsync(List<ChannelOverride> channels, bool isKeep)
    {
        if (channels.Count == 0) yield break;
        var fieldId = isKeep ? "RetainedChannels" : "RemovedChannels";

        var channelMentions = new StringBuilder();
        foreach (var channelOverride in channels)
        {
            var channel = await Context.Guild.GetChannelAsync(channelOverride.ChannelId);
            if (channel == null) continue;

            var mention = channel.GetMention();
            if (channelMentions.Length + mention.Length + 1 > EmbedFieldBuilder.MaxFieldValueLength)
            {
                var mentionsValue = channelMentions.ToString();
                channelMentions.Clear();
                yield return CreateField(fieldId, mentionsValue, false);
            }

            channelMentions.Append(mention).Append(' ');
        }

        if (channelMentions.Length > 0)
            yield return CreateField(fieldId, channelMentions.ToString(), false);
    }

    private async Task<MessageComponent> CreatePaginationComponentsAsync(int currentPage)
    {
        var pagesCount = await ComputeCountOfUnverifies();
        return ComponentsHelper.CreatePaginationComponents(currentPage, pagesCount, "unverify");
    }
}
