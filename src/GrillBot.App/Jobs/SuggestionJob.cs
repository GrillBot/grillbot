﻿using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Managers.EmoteSuggestion;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Services.Repository;
using Quartz;
using EmoteSuggestionManager = GrillBot.Cache.Services.Managers.EmoteSuggestionManager;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private EmoteSuggestionManager CacheManager { get; }
    private EmoteSuggestionHelper Helper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMessageCacheManager MessageCacheManager { get; }
    private new IDiscordClient DiscordClient { get; }

    public SuggestionJob(IServiceProvider serviceProvider, EmoteSuggestionManager cacheManager, EmoteSuggestionHelper helper, GrillBotDatabaseBuilder databaseBuilder,
        IMessageCacheManager messageCacheManager, IDiscordClient discordClient) : base(serviceProvider)
    {
        CacheManager = cacheManager;
        Helper = helper;
        MessageCacheManager = messageCacheManager;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await CacheManager.PurgeExpiredAsync();
        context.Result = await ProcessJobAsync();
    }

    private async Task<string> ProcessJobAsync()
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
            var suggestionsChannel = await Helper.FindEmoteSuggestionsChannelAsync(guild, guildData!, true, "cs");

            if (string.IsNullOrEmpty(guildData!.VoteChannelId))
                throw new ValidationException($"Channel for suggestions ({guildData.VoteChannelId}) is not set.");
            var voteChannel = await guild.GetTextChannelAsync(guildData.VoteChannelId.ToUlong());
            if (voteChannel == null)
                throw new ValidationException($"Channel for suggestion wasn't found. ({guildData.VoteChannelId})");

            if (await MessageCacheManager.GetAsync(suggestion.VoteMessageId!.ToUlong(), voteChannel, forceReload: true) is not IUserMessage message)
                return CreateJobReport(suggestion, "Vote message wasn't found.");

            var thumbsUpReactions = await message.GetReactionUsersAsync(Emojis.ThumbsUp, int.MaxValue).FlattenAsync();
            var thumbsDownReactions = await message.GetReactionUsersAsync(Emojis.ThumbsDown, int.MaxValue).FlattenAsync();

            suggestion.UpVotes = thumbsUpReactions.Count(o => o.IsUser());
            suggestion.DownVotes = thumbsDownReactions.Count(o => o.IsUser());
            suggestion.CommunityApproved = suggestion.UpVotes > suggestion.DownVotes;
            suggestion.VoteFinished = true;

            var fromUser = await DiscordClient.FindUserAsync(suggestion.FromUserId.ToUlong());
            await EmoteSuggestionHelper.SendSuggestionWithEmbedAsync(suggestion, suggestionsChannel, embed: new EmoteSuggestionEmbedBuilder(suggestion, fromUser).Build());
            await message.DeleteAsync();
            return CreateJobReport(suggestion, $"Úspěšně dokončen. ({suggestion.UpVotes}/{suggestion.DownVotes})");
        }
        catch (ValidationException ex)
        {
            return CreateJobReport(suggestion, ex.Message);
        }
    }

    private static string CreateJobReport(Database.Entity.EmoteSuggestion suggestion, string result)
        => $"Id:{suggestion.Id}, Guild:{suggestion.Guild.Name}, FromUser:{suggestion.FromUser.User!.Username}, EmoteName:{suggestion.EmoteName}, Result:{result}";
}
