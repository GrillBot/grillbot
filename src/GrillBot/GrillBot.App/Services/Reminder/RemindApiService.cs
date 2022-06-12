using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Reminder;

public class RemindApiService : ServiceBase
{
    public RemindApiService(GrillBotDatabaseBuilder dbFactory, IMapper mapper) : base(null, dbFactory, null, mapper)
    {
    }

    public async Task<PaginatedResponse<RemindMessage>> GetListAsync(GetReminderListParams parameters)
    {
        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);
        return await PaginatedResponse<RemindMessage>
            .CreateAsync(query, parameters.Pagination, entity => Mapper.Map<RemindMessage>(entity));
    }
}
