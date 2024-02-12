using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.Dashboard;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetUserMeasuresDashboard : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private IUserMeasuresServiceClient UserMeasuresService { get; }

    public GetUserMeasuresDashboard(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts,
        IUserMeasuresServiceClient userMeasuresService) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        UserMeasuresService = userMeasuresService;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var data = await UserMeasuresService.GetDashboardDataAsync();
        var result = await MapAsync(data);

        return ApiResult.Ok(result);
    }

    private async Task<List<DashboardInfoRow>> MapAsync(List<DashboardRow> rows)
    {
        var userIds = rows.Select(o => o.UserId).Distinct().ToList();
        var users = await GetUsersAsync(userIds);

        return rows.ConvertAll(o => new DashboardInfoRow
        {
            Name = FormatUser(users.Find(u => u.Id == o.UserId), o.UserId),
            Result = GetText(o.Type)
        });
    }

    private async Task<List<Database.Entity.User>> GetUsersAsync(List<string> userIds)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.User.GetUsersByIdsAsync(userIds);
    }

    private string GetText(string id)
        => Texts[$"User/UserMeasures/{id}", ApiContext.Language];

    private string FormatUser(Database.Entity.User? user, string defaultValue)
    {
        return user is null ?
            string.Format(GetText("UnknownUser"), defaultValue) :
            $"User({user.Id}/{user.GetDisplayName()})";
    }
}
