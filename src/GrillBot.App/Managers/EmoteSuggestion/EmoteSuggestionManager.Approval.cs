using GrillBot.App.Services.Suggestion;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Managers.EmoteSuggestion;

public partial class EmoteSuggestionManager
{
    public async Task SetApprovalStateAsync(IComponentInteraction interaction, bool approved, IMessageChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var suggestion = await repository.EmoteSuggestion.FindSuggestionByMessageId(interaction.GuildId!.Value, interaction.Message.Id);
        if (suggestion == null)
        {
            await interaction.UpdateAsync(msg => msg.Components = null);
            await interaction.DeferAsync();
            return;
        }

        await SetApprovalStateAsync(new List<Database.Entity.EmoteSuggestion> { suggestion }, approved, channel);
        await repository.CommitAsync();
        await interaction.DeferAsync();
    }

    private async Task SetApprovalStateAsync(IEnumerable<Database.Entity.EmoteSuggestion> suggestions, bool approved, IMessageChannel channel)
    {
        foreach (var suggestion in suggestions)
        {
            if (!suggestion.VoteFinished && suggestion.VoteEndsAt == null)
                suggestion.ApprovedForVote = approved;

            var user = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
            if (await MessageCacheManager.GetAsync(suggestion.SuggestionMessageId.ToUlong(), channel) is IUserMessage message)
            {
                await message.ModifyAsync(msg =>
                {
                    msg.Embed = new EmoteSuggestionEmbedBuilder(suggestion, user).Build();
                    msg.Components = suggestion.ApprovedForVote == true && suggestion.VoteFinished ? null : BuildApprovalButtons();
                });
            }
        }
    }
}
