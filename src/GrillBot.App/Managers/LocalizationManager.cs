using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Models;
using GrillBot.Models.Events.Messages.Embeds;

namespace GrillBot.App.Managers;

public class LocalizationManager(
    ITextsManager _texts,
    IDiscordClient _discordClient,
    DataResolveManager _dataResolve
)
{
    public async Task<EmbedBuilder> CreateLocalizedEmbedAsync(DiscordMessageEmbed original, string language, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        var result = new EmbedBuilder();

        if (!string.IsNullOrEmpty(original.Url?.Key))
            result.WithUrl(await TransformValueAsync(original.Url, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(original.Title?.Key))
            result.WithTitle(await TransformValueAsync(original.Title, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(original.Description?.Key))
            result.WithDescription(await TransformValueAsync(original.Description, language, additionalData, cancellationToken));

        if (original.Author is not null)
            await ProcessAuthorAsync(original, result, language, additionalData, cancellationToken);

        if (original.Color is not null)
            result.WithColor(original.Color.Value);

        if (original.Footer is not null)
            await ProcessFooterAsync(original, result, language, additionalData, cancellationToken);

        if (original.Timestamp is not null)
            result.WithTimestamp(original.Timestamp.Value);

        if (!string.IsNullOrEmpty(original.ImageUrl?.Key))
            result.WithImageUrl(await TransformValueAsync(original.ImageUrl, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(original.ThumbnailUrl?.Key))
            result.WithThumbnailUrl(await TransformValueAsync(result.ThumbnailUrl, language, additionalData, cancellationToken));

        foreach (var field in original.Fields)
            result.AddField(await ProcessFieldAsync(field, language, additionalData, cancellationToken));

        return result;
    }

    private async Task ProcessAuthorAsync(DiscordMessageEmbed original, EmbedBuilder result, string language, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        var author = new EmbedAuthorBuilder();

        if (!string.IsNullOrEmpty(original.Author?.Name?.Key))
            author.WithName(await TransformValueAsync(original.Author.Name, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(original.Author?.Url?.Key))
            author.WithUrl(await TransformValueAsync(original.Author.Url, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(original.Author?.IconUrl?.Key))
            author.WithIconUrl(await TransformValueAsync(original.Author.IconUrl, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(author.IconUrl) || !string.IsNullOrEmpty(author.Name) || !string.IsNullOrEmpty(author.Url))
            result.WithAuthor(author);
    }

    private async Task ProcessFooterAsync(DiscordMessageEmbed original, EmbedBuilder result, string language, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        var footer = new EmbedFooterBuilder();

        if (!string.IsNullOrEmpty(original.Footer?.IconUrl?.Key))
            footer.WithIconUrl(await TransformValueAsync(original.Footer.IconUrl, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(original.Footer?.Text?.Key))
            footer.WithText(await TransformValueAsync(original.Footer.Text, language, additionalData, cancellationToken));

        if (!string.IsNullOrEmpty(footer.Text) || !string.IsNullOrEmpty(footer.IconUrl))
            result.WithFooter(footer);
    }

    private async Task<EmbedFieldBuilder> ProcessFieldAsync(DiscordMessageEmbedField original, string language, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        return new EmbedFieldBuilder()
            .WithName(await TransformValueAsync(original.Name, language, additionalData, cancellationToken))
            .WithValue(await TransformValueAsync(original.Value.ToString() ?? "-", language, additionalData, cancellationToken))
            .WithIsInline(original.IsInline);
    }

    public async Task<string> TransformValueAsync(LocalizedMessageContent value, string language, Dictionary<string, string> additionalData, CancellationToken cancellationToken = default)
    {
        if (value.Key.StartsWith("Bot."))
        {
            switch (value)
            {
                case "Bot.DisplayName":
                    return _discordClient.CurrentUser.GetDisplayName();
                case "Bot.AvatarUrl":
                    return _discordClient.CurrentUser.GetUserAvatarUrl();
                case "Bot.Mention":
                    return _discordClient.CurrentUser.Mention;
            }
        }

        if (value.Key.StartsWith("User."))
        {
            var resolveUserId = value.Key.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1].ToUlong();
            var user = await _dataResolve.GetUserAsync(resolveUserId, cancellationToken);

            if (user is null)
                return value;

            if (value.Key.StartsWith("User.AvatarUrl:"))
                return user.AvatarUrl;
            else if (value.Key.StartsWith("User.DisplayName:"))
                return user.DisplayName;
            else if (value.Key.StartsWith("User.Mention:"))
                return MentionUtils.MentionUser(user.Id.ToUlong());
        }

        if (value.Key.StartsWith("UserDisplayName.") && ulong.TryParse(value.Key.Replace("UserDisplayName.", ""), CultureInfo.InvariantCulture, out var userId))
        {
            var user = await _discordClient.FindUserAsync(userId, cancellationToken);
            return user is null ? value : user.GetDisplayName();
        }

        if (value.Key.StartsWith("DateTime:") && DateTime.TryParse(value.Key.Replace("DateTime:", ""), CultureInfo.InvariantCulture, out var dateTime))
            return dateTime.ToLocalTime().ToCzechFormat();

        var localized = _texts.GetIfExists(value, language).ReplaceIfNullOrEmpty(value);
        return ProcessAdditionalData(value.Key, localized, additionalData);
    }

    private static string ProcessAdditionalData(string rawValue, string localized, Dictionary<string, string> additionalData)
    {
        if (rawValue == "RemindModule/NotifyMessage/Postponed" && additionalData.TryGetValue("PostponeCount", out var postponeCount))
            return localized.FormatWith(postponeCount);

        if (rawValue == "SuggestionModule/CreateSuggestion/TooMuchSuggestions" && additionalData.TryGetValue("MaxSuggestions", out var maxSuggestions))
            return localized.FormatWith(maxSuggestions);

        return localized;
    }
}
