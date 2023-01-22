using GrillBot.Common.Extensions;

namespace GrillBot.App.Managers.EmoteSuggestion;

public partial class EmoteSuggestionManager
{
    private static MessageComponent BuildApprovalButtons()
    {
        return new ComponentBuilder()
            .WithButton("Schválit", "emote_suggestion_approve:true", ButtonStyle.Success)
            .WithButton("Zamítnout", "emote_suggestion_approve:false", ButtonStyle.Danger)
            .Build();
    }

    private static async Task<ITextChannel> FindEmoteSuggestionsChannelAsync(IGuild guild, Database.Entity.Guild dbGuild, bool isFinish)
    {
        if (string.IsNullOrEmpty(dbGuild.EmoteSuggestionChannelId))
        {
            throw new ValidationException(isFinish
                ? $"Nepodařilo se najít kanál pro návrhy ({dbGuild.EmoteSuggestionChannelId})."
                : "Tvůj návrh na nelze nyní zpracovat, protože není určen kanál pro návrhy.");
        }

        var channel = await guild.GetTextChannelAsync(dbGuild.EmoteSuggestionChannelId.ToUlong());

        if (channel == null)
        {
            throw new ValidationException(isFinish
                ? $"Nepodařilo se najít kanál pro návrhy ({dbGuild.EmoteSuggestionChannelId})."
                : "Tvůj návrh na emote nelze nyní kvůli technickým důvodům zpracovat.");
        }

        return channel;
    }

    private static async Task<IUserMessage> SendSuggestionWithEmbedAsync(Database.Entity.EmoteSuggestion suggestion, IMessageChannel channel, string msg = null, Embed embed = null)
    {
        using (var ms = new MemoryStream(suggestion.ImageData))
        {
            var attachment = new FileAttachment(ms, suggestion.Filename);
            var allowedMentions = new AllowedMentions(AllowedMentionTypes.None);

            return await channel.SendFileAsync(attachment, msg, embed: embed, allowedMentions: allowedMentions);
        }
    }
}
