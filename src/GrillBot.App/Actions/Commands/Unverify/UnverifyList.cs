using GrillBot.App.Infrastructure.Embeds;
using GrillBot.App.Modules.Implementations.Unverify;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Database.Enums;
using UnverifyService;
using UnverifyService.Models.Request;
using UnverifyService.Models.Response;

namespace GrillBot.App.Actions.Commands.Unverify;

public class UnverifyList(
    GrillBotDatabaseBuilder _databaseBuilder,
    ITextsManager _texts,
    FormatHelper _formatHelper,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : CommandAction
{
    public async Task<(Embed embed, MessageComponent? paginationComponent)> ProcessAsync(int page)
    {
        var request = CreateRequest(false, page);
        var unverifyList = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetActiveUnverifyListAsync(request, ctx.CancellationToken)
        );

        if (unverifyList.TotalItemsCount == 0)
            throw new NotFoundException(_texts["Unverify/ListEmbed/NoUnverify", Locale]);

        var activeUnverify = (await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetActiveUnverifyDetailAsync(
                unverifyList.Data[0].GuildId.ToUlong(),
                unverifyList.Data[0].ToUserId.ToUlong(),
                ctx.CancellationToken
            )
        ))!;

        using var repository = _databaseBuilder.CreateRepository();

        var hiddenChannels = await repository.Channel.GetAllChannelsAsync([Context.Guild.Id.ToString()], true, true, ChannelFlag.StatsHidden);
        var hiddenChannelIds = hiddenChannels.Select(o => o.ChannelId).ToHashSet();
        var user = await Context.Guild.GetUserAsync(unverifyList.Data[0].ToUserId.ToUlong());
        var removedChannels = activeUnverify.RemovedChannels.FindAll(o => !hiddenChannelIds.Contains(o.ChannelId));
        var keepedChannels = activeUnverify.KeepedChannels.FindAll(o => !hiddenChannelIds.Contains(o.ChannelId));

        var embed = await CreateEmbedAsync(activeUnverify, user, page, unverifyList.Data[0].IsSelfUnverify, keepedChannels, removedChannels);
        var paginationComponents = await CreatePaginationComponentsAsync(page);
        return (embed, paginationComponents);
    }

    public async Task<int> ComputeCountOfUnverifies()
    {
        var request = CreateRequest(true, 0);

        var unverifyList = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetActiveUnverifyListAsync(request, ctx.CancellationToken)
        );

        return Convert.ToInt32(unverifyList.TotalItemsCount);
    }

    private ActiveUnverifyListRequest CreateRequest(bool onlyCount, int page)
    {
        return new ActiveUnverifyListRequest
        {
            GuildId = Context.Guild.Id.ToString(),
            Pagination = new Core.Models.Pagination.PaginatedParams
            {
                Page = page,
                PageSize = 1,
                OnlyCount = onlyCount
            },
            Sort = new Core.Models.SortParameters
            {
                Descending = false,
                OrderBy = "StartAt"
            }
        };
    }

    private async Task<Embed> CreateEmbedAsync(
        UnverifyDetail unverify,
        IGuildUser user,
        int page,
        bool isSelfUnverify,
        List<ChannelOverride> keepedChannels,
        List<ChannelOverride> removedChannels
    )
    {
        var color = unverify.KeepedRoles.Concat(unverify.RemovedRoles)
            .Select(roleId => user.Guild.GetRole(roleId.ToUlong()))
            .Where(o => o is not null && o.Color != Color.Default)
            .OrderByDescending(o => o.Position)
            .Select(o => o.Color)
            .FirstOrDefault();

        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithAuthor(user)
            .WithMetadata(new UnverifyListMetadata { Page = page })
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithTitle(_texts["Unverify/ListEmbed/Title", Locale]);

        await foreach (var field in CreateEmbedFieldsAsync(unverify, isSelfUnverify, user, keepedChannels, removedChannels))
            embed.AddField(field);
        return embed.Build();
    }

    private async IAsyncEnumerable<EmbedFieldBuilder> CreateEmbedFieldsAsync(
        UnverifyDetail unverify,
        bool isSelfUnverify,
        IGuildUser user,
        List<ChannelOverride> keepedChannels,
        List<ChannelOverride> removedChannels
    )
    {
        yield return CreateField("StartAt", unverify.StartAtUtc.ToTimestampMention(), true);
        yield return CreateField("EndAt", unverify.EndAtUtc.ToTimestampMention(), true);
        yield return CreateField("EndFor", FormatEndDate(unverify.EndAtUtc), true);
        yield return CreateField("Selfunverify", _formatHelper.FormatBoolean("Unverify/ListEmbed/Boolean", Locale, isSelfUnverify), true);

        if (!string.IsNullOrEmpty(unverify.Reason))
            yield return CreateField("Reason", unverify.Reason, false);

        var rolesToKeep = unverify.KeepedRoles
            .Select(roleId => user.Guild.GetRole(roleId.ToUlong()))
            .Where(role => role is not null);

        foreach (var roleGroupField in CreateRolesList(rolesToKeep, true))
            yield return roleGroupField;

        var rolesToRemove = unverify.RemovedRoles
            .Select(roleId => user.Guild.GetRole(roleId.ToUlong()))
            .Where(role => role is not null);

        foreach (var roleGroupField in CreateRolesList(rolesToRemove, false))
            yield return roleGroupField;

        await foreach (var channelGroupField in CreateChannelsListAsync(keepedChannels, true))
            yield return channelGroupField;

        await foreach (var channelGroupField in CreateChannelsListAsync(removedChannels, false))
            yield return channelGroupField;
    }

    private string FormatEndDate(DateTime endAt)
    {
        var sign = endAt <= DateTime.UtcNow ? "-" : "";
        return sign + (endAt - DateTime.UtcNow).Humanize(culture: _texts.GetCulture(Locale));
    }

    private EmbedFieldBuilder CreateField(string fieldId, string value, bool inline)
        => new EmbedFieldBuilder().WithName(_texts[$"Unverify/ListEmbed/Fields/{fieldId}", Locale]).WithValue(value).WithIsInline(inline);

    private IEnumerable<EmbedFieldBuilder> CreateRolesList(IEnumerable<IRole> roles, bool isKeep)
    {
        if (!roles.Any())
            yield break;

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
            var channel = await Context.Guild.GetChannelAsync(channelOverride.ChannelId.ToUlong());
            if (channel is null)
                continue;

            var hyperlink = channel.GetHyperlink();
            if (channelMentions.Length + hyperlink.Length + 1 > EmbedFieldBuilder.MaxFieldValueLength)
            {
                var mentionsValue = channelMentions.ToString();
                channelMentions.Clear();
                yield return CreateField(fieldId, mentionsValue, false);
            }

            channelMentions.Append(hyperlink).Append(", ");
        }

        var channelMentionsText = channelMentions.ToString().Trim();
        if (channelMentionsText.EndsWith(','))
            channelMentionsText = channelMentionsText[..^1];

        if (channelMentionsText.Length > 0)
            yield return CreateField(fieldId, channelMentionsText, false);
    }

    private async Task<MessageComponent?> CreatePaginationComponentsAsync(int currentPage)
    {
        var pagesCount = await ComputeCountOfUnverifies();
        return ComponentsHelper.CreatePaginationComponents(currentPage, pagesCount, "unverify");
    }
}
