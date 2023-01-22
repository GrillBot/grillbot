using System.Reflection;
using Discord.Interactions;
using GrillBot.App.Helpers;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Commands.EmoteSuggestion;

public class InitSuggestion : CommandAction
{
    private ITextsManager Texts { get; }
    private DownloadHelper DownloadHelper { get; }
    private EmoteSuggestionManager SuggestionCacheManager { get; }

    private Emote? Emote { get; set; }
    private IAttachment? Attachment { get; set; }

    public InitSuggestion(ITextsManager texts, DownloadHelper downloadHelper, EmoteSuggestionManager cacheManager)
    {
        Texts = texts;
        DownloadHelper = downloadHelper;
        SuggestionCacheManager = cacheManager;
    }

    public async Task<Modal> ProcessAsync(IEmote? emote, IAttachment? attachment)
    {
        ValidateData(emote, attachment);

        var (filename, content) = await GetDataAsync();
        var sessionId = await SuggestionCacheManager.InitAsync(filename, content);
        var emoteName = Path.GetFileNameWithoutExtension(filename);

        return CreateModal(sessionId, emoteName);
    }

    private void ValidateData(IEmote? emoteData, IAttachment? attachment)
    {
        Emote = emoteData as Emote;
        Attachment = attachment;

        if (Emote == null && Attachment == null)
            throw new ValidationException(GetText("NoEmoteAndAttachment"));

        if (Emote != null && Context.Guild.Emotes.Any(o => o.Id == Emote.Id))
            throw new ValidationException(GetText("EmoteExistsInGuild"));
    }

    private async Task<(string filename, byte[] content)> GetDataAsync()
    {
        string filename;
        byte[]? content;

        if (Emote != null)
        {
            filename = Emote.Name + Path.GetExtension(Path.GetFileName(Emote.Url));
            content = await DownloadHelper.DownloadFileAsync(Emote.Url);
        }
        else
        {
            filename = Attachment!.Filename;
            content = await DownloadHelper.DownloadAsync(Attachment);
        }

        if (content == null)
            throw new InvalidOperationException("CDN not provided content for attachment or emote.");
        return (filename, content);
    }

    private Modal CreateModal(string sessionId, string emoteName)
    {
        return new ModalBuilder(GetText("ModalTitle"), $"suggestions_emote:{sessionId}")
            .AddTextInput(CreateTextInput(nameof(EmoteSuggestionModal.EmoteName), GetText("ModalEmoteName"), defaultValue: emoteName))
            .AddTextInput(CreateTextInput(nameof(EmoteSuggestionModal.EmoteDescription), GetText("ModalEmoteDescription"), GetText("ModalEmoteDescriptionPlaceholder")))
            .Build();
    }

    private static TextInputBuilder CreateTextInput(string propertyName, string? label = null, string? placeholder = null, string? defaultValue = null)
    {
        var property = typeof(EmoteSuggestionModal).GetProperty(propertyName);
        if (property == null) throw new ArgumentException($"Property {propertyName} wasn't found.");

        var isRequired = property.GetCustomAttribute<RequiredInputAttribute>()?.IsRequired == true;
        var inputAttribute = property.GetCustomAttribute<ModalTextInputAttribute>();
        if (inputAttribute == null) throw new NotFoundException("ModalTextInput attribute is required.");

        label ??= property.GetCustomAttribute<InputLabelAttribute>()!.Label;
        placeholder ??= inputAttribute.Placeholder;

        return new TextInputBuilder(label, inputAttribute.CustomId, inputAttribute.Style, placeholder, inputAttribute.MinLength, inputAttribute.MaxLength, isRequired, defaultValue);
    }

    private string GetText(string id)
        => Texts[$"SuggestionModule/SuggestEmote/{id}", Locale];
}
