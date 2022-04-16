using Discord.Interactions;

namespace GrillBot.App.Modules.Implementations.Suggestion;

public class FeatureSuggestionModal : IModal
{
    public string Title => "Podání návrhu na novou feature v GrillBot";

    [RequiredInput]
    [InputLabel("Název feature")]
    [ModalTextInput("suggestions_feature_name", TextInputStyle.Short, minLength: 5)]
    public string Name { get; set; }

    [RequiredInput]
    [InputLabel("Popis feature")]
    [ModalTextInput("suggestions_feature_description", TextInputStyle.Paragraph)]
    public string Description { get; set; }

    public string User { get; set; }
}
