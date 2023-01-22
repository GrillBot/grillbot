using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Helpers;

public class EmoteSuggestionHelper
{
    private ITextsManager Texts { get; }

    public EmoteSuggestionHelper(ITextsManager texts)
    {
        Texts = texts;
    }

    public MessageComponent CreateApprovalButtons(string locale)
    {
        return new ComponentBuilder()
            .WithButton(Texts["SuggestionModule/ApprovalButtons/Approve", locale], "emote_suggestion_approve:true", ButtonStyle.Success)
            .WithButton(Texts["SuggestionModule/ApprovalButtons/Decline", locale], "emote_suggestion_approve:false", ButtonStyle.Danger)
            .Build();
    }

    public async Task<ITextChannel> FindEmoteSuggestionsChannelAsync(IGuild guild, Database.Entity.Guild dbGuild, bool isFinish, string locale)
    {
        if (string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
        {
            throw new ValidationException(isFinish
                ? Texts["SuggestionModule/EmoteSuggestionChannelNotSet/IsFinish", locale].FormatWith(dbGuild.EmoteSuggestionChannelId)
                : Texts["SuggestionModule/EmoteSuggestionChannelNotSet/IsNotFinish", locale]
            );
        }

        var channel = await guild.GetTextChannelAsync(dbGuild.EmoteSuggestionChannelId.ToUlong());
        if (channel == null)
        {
            throw new ValidationException(isFinish
                ? Texts["SuggestionModule/EmoteSuggestionChannelNotFound/IsFinish", locale].FormatWith(dbGuild.EmoteSuggestionChannelId)
                : Texts["SuggestionModule/EmoteSuggestionChannelNotFound/NotFinish", locale].FormatWith(dbGuild.EmoteSuggestionChannelId)
            );
        }

        return channel;
    }

    public static async Task<IUserMessage> SendSuggestionWithEmbedAsync(Database.Entity.EmoteSuggestion suggestion, IMessageChannel channel, string? msg = null, Embed? embed = null)
    {
        // ReSharper disable once ConvertToUsingDeclaration
        using (var ms = new MemoryStream(suggestion.ImageData))
        {
            var attachment = new FileAttachment(ms, suggestion.Filename);
            var allowedMentions = new AllowedMentions(AllowedMentionTypes.None);

            return await channel.SendFileAsync(attachment, msg, embed: embed, allowedMentions: allowedMentions);
        }
    }
}
