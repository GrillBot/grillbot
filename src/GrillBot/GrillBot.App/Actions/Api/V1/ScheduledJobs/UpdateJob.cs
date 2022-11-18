using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using Quartz;
using Quartz.Impl.Matchers;

namespace GrillBot.App.Actions.Api.V1.ScheduledJobs;

public class UpdateJob : ApiAction
{
    private DataCacheManager DataCacheManager { get; }
    private ISchedulerFactory SchedulerFactory { get; }
    private ITextsManager Texts { get; }

    public UpdateJob(ApiRequestContext apiContext, DataCacheManager dataCacheManager, ISchedulerFactory schedulerFactory, ITextsManager texts) : base(apiContext)
    {
        DataCacheManager = dataCacheManager;
        SchedulerFactory = schedulerFactory;
        Texts = texts;
    }

    public async Task ProcessAsync(string name, bool enabled)
    {
        await ValidateJobAsync(name);

        var data = await DataCacheManager.GetValueAsync("DisabledJobs");
        if (string.IsNullOrEmpty(data)) data = "[]";

        var disabledJobs = JsonConvert.DeserializeObject<List<string>>(data)!;
        if (enabled) disabledJobs.Remove(name);
        else disabledJobs.Add(name);

        var newData = JsonConvert.SerializeObject(disabledJobs, Formatting.None);
        if (data != newData)
            await DataCacheManager.SetValueAsync("DisabledJobs", newData, DateTime.MaxValue);
    }

    private async Task ValidateJobAsync(string name)
    {
        var scheduler = await SchedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

        if (!jobKeys.Any(o => o.Name == name))
            throw new NotFoundException(Texts["Jobs/NotFound", ApiContext.Language].FormatWith(name));
    }
}
