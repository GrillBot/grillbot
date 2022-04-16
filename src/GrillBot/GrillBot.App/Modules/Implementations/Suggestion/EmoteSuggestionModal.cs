using Discord.Interactions;

namespace GrillBot.App.Modules.Implementations.Suggestion;

public class EmoteSuggestionModal : IModal
{
    public string Title => "Podání návrhu na nový emote";

    [RequiredInput]
    [InputLabel("Název emote")]
    [ModalTextInput("suggestions_emote_name", TextInputStyle.Short, minLength: 2, maxLength: 50)]
    public string EmoteName { get; set; }

    [InputLabel("Popis")]
    [ModalTextInput("suggestions_emote_description", TextInputStyle.Paragraph, placeholder: "Něco k doplnění? Co emote vyjadřuje? ...")]
    public string EmoteDescription { get; set; }

    public EmoteSuggestionModal() { }

    public EmoteSuggestionModal(IEmote emote)
    {
        EmoteName = emote?.Name;
    }
}
