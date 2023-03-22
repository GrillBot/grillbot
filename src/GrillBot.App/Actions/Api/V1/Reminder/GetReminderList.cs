using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Reminder;

namespace GrillBot.App.Actions.Api.V1.Reminder;

public class GetReminderList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetReminderList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<RemindMessage>> ProcessAsync(GetReminderListParams parameters)
    {
        CheckAndSetPublicAccess(parameters);
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Remind.GetRemindListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<RemindMessage>.CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<RemindMessage>(entity)));
    }

    private void CheckAndSetPublicAccess(GetReminderListParams parameters)
    {
        if (!ApiContext.IsPublic()) return;

        parameters.ToUserId = ApiContext.GetUserId().ToString();
        parameters.OriginalMessageId = null;

        if (parameters.Sort.OrderBy == "ToUser")
            parameters.Sort.OrderBy = "Id";
    }
}
