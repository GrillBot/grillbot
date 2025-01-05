using Discord.Net;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Core.Extensions;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class CacheCleanerJob : CleanerJobBase
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
        await ClearProfilePicturesAsync(repository, reportFields);

        context.Result = FormatReportFromFields(reportFields);
    }

    private async Task ClearProfilePicturesAsync(GrillBotCacheRepository cacheRepository, ICollection<string> report)
    {
        if (!InitManager.Get()) return;

        var cleared = 0;
        var processedUsers = new List<ulong>();
        var profilePictures = await cacheRepository.ProfilePictureRepository.GetAllProfilePicturesAsync();

        foreach (var userProfilePictures in profilePictures.GroupBy(o => o.UserId))
        {
            var userId = userProfilePictures.Key.ToUlong();

            try
            {
                var user = await DiscordClient.GetUserAsync(userId);
                if (user?.Username.StartsWith("Deleted User", StringComparison.InvariantCultureIgnoreCase) != false)
                {
                    cacheRepository.RemoveCollection(userProfilePictures);
                    cleared += userProfilePictures.Count();
                }
                else
                {
                    var avatarId = string.IsNullOrEmpty(user.AvatarId) ? user.Id.ToString() : user.AvatarId;

                    foreach (var picture in userProfilePictures.Where(p => p.AvatarId != avatarId))
                    {
                        cacheRepository.Remove(picture);
                        cleared++;
                    }
                }
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.ServiceUnavailable)
            {
                continue;
            }
            catch (TimeoutException)
            {
                continue;
            }
            catch (HttpException ex) when (ex.DiscordCode is not null && ex.DiscordCode == DiscordErrorCode.UnknownUser)
            {
                cacheRepository.RemoveCollection(userProfilePictures);
                cleared += userProfilePictures.Count();
            }

            processedUsers.Add(userId);
        }

        if (cleared == 0) return;
        await cacheRepository.CommitAsync();
        report.Add($"ProfilePictures: (Cleared: {cleared}, Users: {processedUsers.Count}, TotalPictures: {profilePictures.Count})");
    }
}
