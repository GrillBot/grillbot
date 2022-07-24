namespace GrillBot.App.Services.Suggestion;

public partial class EmoteSuggestionService
{
    public async Task OnMessageDeletedAsync(IUserMessage message, IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var suggestion = await repository.EmoteSuggestion.FindSuggestionByMessageId(guild.Id, message.Id);

        if (suggestion == null) return;
        if (suggestion.VoteFinished || suggestion.VoteEndsAt != null) return; // Cannot delete processed suggestions.

        repository.Remove(suggestion);
        await repository.CommitAsync();
    }
}
