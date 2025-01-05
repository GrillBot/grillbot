using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using Quartz;
using Quartz.Impl.Matchers;

namespace GrillBot.App.Actions.Api.V1.ScheduledJobs;

public class UpdateJob : ApiAction
{
    private readonly DataCacheManager _dataCacheManager;
    private ISchedulerFactory SchedulerFactory { get; }
    private ITextsManager Texts { get; }

    public UpdateJob(ApiRequestContext apiContext, DataCacheManager dataCacheManager, ISchedulerFactory schedulerFactory, ITextsManager texts) : base(apiContext)
    {
        _dataCacheManager = dataCacheManager;
        SchedulerFactory = schedulerFactory;
        Texts = texts;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var name = (string)Parameters[0]!;
        var enabled = (bool)Parameters[1]!;

        await ValidateJobAsync(name);

        var disabledJobs = await _dataCacheManager.GetValueAsync<List<string>>("DisabledJobs");
        disabledJobs ??= new List<string>();

        if (enabled)
            disabledJobs.Remove(name);
        else
            disabledJobs.Add(name);

        await _dataCacheManager.SetValueAsync("DisabledJobs", disabledJobs, null);
        return ApiResult.Ok();
    }

    private async Task ValidateJobAsync(string name)
    {
        var scheduler = await SchedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

        if (!jobKeys.Any(o => o.Name == name))
            throw new NotFoundException(Texts["Jobs/NotFound", ApiContext.Language].FormatWith(name));
    }
}
