using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Managers;

public class LocalizationManager(
    ITextsManager _texts,
    IDiscordClient _discordClient,
    DataResolveManager _dataResolve
)
{
    public async Task<EmbedBuilder> CreateLocalizedEmbedAsync(EmbedBuilder original, string language, Dictionary<string, string> additionalData)
    {
        var result = new EmbedBuilder();

        if (!string.IsNullOrEmpty(original.Url))
            result.WithUrl(await TransformValueAsync(original.Url, language, additionalData));

        if (!string.IsNullOrEmpty(original.Title))
            result.WithTitle(await TransformValueAsync(original.Title, language, additionalData));

        if (!string.IsNullOrEmpty(original.Description))
            result.WithDescription(await TransformValueAsync(original.Description, language, additionalData));

        if (original.Author is not null)
            await ProcessAuthorAsync(original, result, language, additionalData);

        if (original.Color is not null)
            result.WithColor(original.Color.Value);

        if (original.Footer is not null)
            await ProcessFooterAsync(original, result, language, additionalData);

        if (original.Timestamp is not null)
            result.WithTimestamp(original.Timestamp.Value);

        if (!string.IsNullOrEmpty(original.ImageUrl))
            result.WithImageUrl(await TransformValueAsync(original.ImageUrl, language, additionalData));

        if (!string.IsNullOrEmpty(original.ThumbnailUrl))
            result.WithThumbnailUrl(await TransformValueAsync(result.ThumbnailUrl, language, additionalData));

        foreach (var field in original.Fields)
            result.AddField(await ProcessFieldAsync(field, language, additionalData));

        return result;
    }

    private async Task ProcessAuthorAsync(EmbedBuilder original, EmbedBuilder result, string language, Dictionary<string, string> additionalData)
    {
        var author = new EmbedAuthorBuilder();

        if (!string.IsNullOrEmpty(original.Author.Name))
            author.WithName(await TransformValueAsync(original.Author.Name, language, additionalData));

        if (!string.IsNullOrEmpty(original.Author.Url))
            author.WithUrl(await TransformValueAsync(original.Author.Url, language, additionalData));

        if (!string.IsNullOrEmpty(original.Author.IconUrl))
            author.WithIconUrl(await TransformValueAsync(original.Author.IconUrl, language, additionalData));

        if (!string.IsNullOrEmpty(author.IconUrl) || !string.IsNullOrEmpty(author.Name) || !string.IsNullOrEmpty(author.Url))
            result.WithAuthor(author);
    }

    private async Task ProcessFooterAsync(EmbedBuilder original, EmbedBuilder result, string language, Dictionary<string, string> additionalData)
    {
        var footer = new EmbedFooterBuilder();

        if (!string.IsNullOrEmpty(original.Footer.IconUrl))
            footer.WithIconUrl(await TransformValueAsync(original.Footer.IconUrl, language, additionalData));

        if (!string.IsNullOrEmpty(original.Footer.Text))
            footer.WithText(await TransformValueAsync(original.Footer.Text, language, additionalData));

        if (!string.IsNullOrEmpty(footer.Text) || !string.IsNullOrEmpty(footer.IconUrl))
            result.WithFooter(footer);
    }

    private async Task<EmbedFieldBuilder> ProcessFieldAsync(EmbedFieldBuilder original, string language, Dictionary<string, string> additionalData)
    {
        return new EmbedFieldBuilder()
            .WithName(await TransformValueAsync(original.Name, language, additionalData))
            .WithValue(await TransformValueAsync(original.Value.ToString() ?? "-", language, additionalData))
            .WithIsInline(original.IsInline);
    }

    public async Task<string> TransformValueAsync(string value, string language, Dictionary<string, string> additionalData)
    {
        if (value.StartsWith("Bot."))
        {
            switch (value)
            {
                case "Bot.DisplayName":
                    return _discordClient.CurrentUser.GetDisplayName();
                case "Bot.AvatarUrl":
                    return _discordClient.CurrentUser.GetUserAvatarUrl();
            }
        }

        if (value.StartsWith("User."))
        {
            var resolveUserId = value.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1].ToUlong();
            var user = await _dataResolve.GetUserAsync(resolveUserId);

            if (user is null)
                return value;

            if (value.StartsWith("User.AvatarUrl:"))
                return user.AvatarUrl;
            else if (value.StartsWith("User.DisplayName:"))
                return user.DisplayName;
        }

        if (value.StartsWith("UserDisplayName.") && ulong.TryParse(value.Replace("UserDisplayName.", ""), CultureInfo.InvariantCulture, out var userId))
        {
            var user = await _discordClient.FindUserAsync(userId);
            return user is null ? value : user.GetDisplayName();
        }

        var localized = _texts.GetIfExists(value, language).ReplaceIfNullOrEmpty(value);
        return ProcessAdditionalData(value, localized, additionalData);
    }

    private static string ProcessAdditionalData(string rawValue, string localized, Dictionary<string, string> additionalData)
    {
        if (rawValue == "RemindModule/NotifyMessage/Postponed" && additionalData.TryGetValue("PostponeCount", out var postponeCount))
            return localized.FormatWith(postponeCount);

        return localized;
    }
}
