using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using Emote.Models.Events.Suggestions;

namespace GrillBot.App.Actions.Commands.Emotes.Suggestions;

public class CreateEmoteSuggestionAction(
    DownloadHelper _downloadHelper,
    IRabbitPublisher _rabbitPublisher,
    ITextsManager _texts
) : CommandAction
{
    public string? ErrorMessage { get; private set; }

    public async Task ProcessAsync(string reason, string? name, IEmote? emote, IAttachment? attachment)
    {
        var data = PrepareData(reason, name, emote, attachment);
        if (data is null)
            return;

        var image = await GetImageAsync(data.Value.emote, data.Value.attachment);
        var isAnimated = IsAnimated(data.Value.emote, data.Value.attachment);

        var payload = new EmoteSuggestionRequestPayload(
            data.Value.name,
            data.Value.reason,
            image,
            Context.Guild.Id,
            Context.User.Id,
            DateTime.UtcNow,
            isAnimated,
            Context.Interaction.UserLocale
        );

        await _rabbitPublisher.PublishAsync(payload);
    }

    private (string reason, string name, Discord.Emote? emote, IAttachment? attachment)? PrepareData(
        string reason,
        string? name,
        IEmote? emote,
        IAttachment? attachment
    )
    {
        if (string.IsNullOrEmpty(reason))
        {
            ErrorMessage = GetText("MissingReason");
            return null;
        }

        if (reason.Length > EmbedFieldBuilder.MaxFieldValueLength)
        {
            ErrorMessage = GetText("ReasonTooLong");
            return null;
        }

        var _emote = emote as Discord.Emote;
        if (_emote is null && attachment is null)
        {
            ErrorMessage = GetText("MissingEmoteOrAttachment");
            return null;
        }

        if (_emote is not null && attachment is not null)
        {
            ErrorMessage = GetText("BothEmoteAndAttachment");
            return null;
        }

        if (_emote is not null && string.IsNullOrEmpty(name))
        {
            name = _emote.Name;
        }

        if (attachment is not null && string.IsNullOrEmpty(name))
        {
            ErrorMessage = GetText("MissingName");
            return null;
        }

        return (reason, name!, _emote, attachment);
    }

    private async Task<byte[]> GetImageAsync(Discord.Emote? emote, IAttachment? attachment)
    {
        byte[]? image = null;

        if (emote is not null)
        {
            image = await _downloadHelper.DownloadFileAsync(emote.Url);
        }

        if (attachment is not null)
        {
            image = await _downloadHelper.DownloadAsync(attachment);
        }

        ArgumentNullException.ThrowIfNull(image);
        return image;
    }

    private static bool IsAnimated(Discord.Emote? emote, IAttachment? attachment)
        => emote?.Animated == true || Path.GetExtension(attachment?.Filename) == ".gif";

    private string GetText(string id)
        => _texts[$"SuggestionModule/CreateSuggestion/{id}", Locale];
}
