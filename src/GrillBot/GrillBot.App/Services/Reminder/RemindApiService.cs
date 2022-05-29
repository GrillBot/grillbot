using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Reminder;

namespace GrillBot.App.Services.Reminder;

public class RemindApiService : ServiceBase
{
    public RemindApiService(GrillBotDatabaseFactory dbFactory, IMapper mapper) : base(null, dbFactory, null, mapper)
    {
    }

    public async Task<PaginatedResponse<RemindMessage>> GetListAsync(GetReminderListParams parameters, CancellationToken cancellationToken = default)
    {
        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);
        return await PaginatedResponse<RemindMessage>
            .CreateAsync(query, parameters.Pagination, entity => Mapper.Map<RemindMessage>(entity), cancellationToken);
    }
}
