using GrillBot.App.Services.Suggestion;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Managers.EmoteSuggestion;

public partial class EmoteSuggestionManager
{
    public async Task<string> ProcessJobAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var report = new StringBuilder();
        foreach (var guild in await DiscordClient.GetGuildsAsync())
        {
            var suggestions = await repository.EmoteSuggestion.FindSuggestionsForFinishAsync(guild);
            if (suggestions.Count == 0)
                continue;

            foreach (var suggestion in suggestions)
            {
                var suggestionReport = await FinishVoteForSuggestionAsync(guild, repository, suggestion);
                report.AppendLine(suggestionReport);
            }

            await repository.CommitAsync();
        }

        return report.ToString();
    }

    private async Task<string> FinishVoteForSuggestionAsync(IGuild guild, GrillBotRepository repository, Database.Entity.EmoteSuggestion suggestion)
    {
        try
        {
            var guildData = await repository.Guild.FindGuildAsync(guild);
            var suggestionsChannel = await FindEmoteSuggestionsChannelAsync(guild, guildData, true);

            if (string.IsNullOrEmpty(guildData!.VoteChannelId))
                throw new ValidationException($"Není nastaven kanál pro hlasování ({guildData.VoteChannelId})");
            var voteChannel = await guild.GetTextChannelAsync(guildData.VoteChannelId.ToUlong());
            if (voteChannel == null)
                throw new ValidationException($"Nepodařilo se najít kanál pro hlasování ({guildData.VoteChannelId})");

            if (await MessageCacheManager.GetAsync(suggestion.VoteMessageId!.ToUlong(), voteChannel, forceReload: true) is not IUserMessage message)
                return CreateJobReport(suggestion, "Nepodařilo se najít hlasovací zprávu.");

            var thumbsUpReactions = await message.GetReactionUsersAsync(Emojis.ThumbsUp, int.MaxValue).FlattenAsync();
            var thumbsDownReactions = await message.GetReactionUsersAsync(Emojis.ThumbsDown, int.MaxValue).FlattenAsync();

            suggestion.UpVotes = thumbsUpReactions.Count(o => o.IsUser());
            suggestion.DownVotes = thumbsDownReactions.Count(o => o.IsUser());
            suggestion.CommunityApproved = suggestion.UpVotes > suggestion.DownVotes;
            suggestion.VoteFinished = true;

            var fromUser = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
            await SendSuggestionWithEmbedAsync(suggestion, suggestionsChannel, embed: new EmoteSuggestionEmbedBuilder(suggestion, fromUser).Build());
            await message.DeleteAsync();
            return CreateJobReport(suggestion, $"Úspěšně dokončen. ({suggestion.UpVotes}/{suggestion.DownVotes})");
        }
        catch (ValidationException ex)
        {
            return CreateJobReport(suggestion, ex.Message);
        }
    }

    private static string CreateJobReport(Database.Entity.EmoteSuggestion suggestion, string result)
        => $"Id:{suggestion.Id}, Guild:{suggestion.Guild!.Name}, FromUser:{suggestion.FromUser!.User!.Username}, EmoteName:{suggestion.EmoteName}, Result:{result}";
}
