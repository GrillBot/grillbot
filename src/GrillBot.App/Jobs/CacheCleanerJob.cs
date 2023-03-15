using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Repository;
using GrillBot.Core.Extensions;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class CacheCleanerJob : Job
{
    private GrillBotCacheBuilder CacheBuilder { get; }

    public CacheCleanerJob(IServiceProvider serviceProvider, GrillBotCacheBuilder cacheBuilder) : base(serviceProvider)
    {
        CacheBuilder = cacheBuilder;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reportFields = new List<string>();

        await using var repository = CacheBuilder.CreateRepository();

        await ClearDataCacheAsync(repository, reportFields);
        await ClearEmoteSuggestionsAsync(repository, reportFields);
        await ClearProfilePicturesAsync(repository, reportFields);

        if (reportFields.Count > 0)
            context.Result = $"CacheCleaner({string.Join(", ", reportFields)})";
    }

    private static async Task ClearDataCacheAsync(GrillBotCacheRepository cacheRepository, ICollection<string> report)
    {
        var cleared = await cacheRepository.DataCache.DeleteExpiredAsync();
        if (cleared > 0)
            report.Add($"DataCache: {cleared}");
    }

    private static async Task ClearEmoteSuggestionsAsync(GrillBotCacheRepository cacheRepository, ICollection<string> report)
    {
        var cleared = await cacheRepository.EmoteSuggestion.PurgeExpiredAsync(EmoteSuggestionManager.ValidHours);
        if (cleared > 0)
            report.Add($"EmoteSuggestions: {cleared}");
    }

    private async Task ClearProfilePicturesAsync(GrillBotCacheRepository cacheRepository, ICollection<string> report)
    {
        if (!InitManager.Get()) return;

        var cleared = 0;
        var processedUsers = new HashSet<ulong>();
        var profilePictures = await cacheRepository.ProfilePictureRepository.GetAllProfilePicturesAsync();

        foreach (var profilePicture in profilePictures)
        {
            var userId = profilePicture.UserId.ToUlong();
            var user = await DiscordClient.GetUserAsync(userId);
            if (user == null)
            {
                cacheRepository.Remove(profilePicture);
                cleared++;
            }
            else
            {
                var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Discriminator : user.AvatarId;
                if (avatarId != profilePicture.AvatarId)
                {
                    cacheRepository.Remove(profilePicture);
                    cleared++;
                }
            }

            processedUsers.Add(userId);
        }

        if (cleared == 0) return;
        await cacheRepository.CommitAsync();
        report.Add($"ProfilePictures: (Cleared: {cleared}, Users: {processedUsers.Count})");
    }
}
