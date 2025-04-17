using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.Dashboard;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetUserMeasuresDashboard : ApiAction
{
    private ITextsManager Texts { get; }

    private readonly DataResolveManager _dataResolveManager;
    private readonly IServiceClientExecutor<IUserMeasuresServiceClient> _userMeasuresService;

    public GetUserMeasuresDashboard(ApiRequestContext apiContext, ITextsManager texts, IServiceClientExecutor<IUserMeasuresServiceClient> userMeasuresService,
        DataResolveManager dataResolveManager) : base(apiContext)
    {
        Texts = texts;
        _userMeasuresService = userMeasuresService;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var data = await _userMeasuresService.ExecuteRequestAsync((c, ctx) => c.GetDashboardDataAsync(ctx.CancellationToken));
        var result = await MapAsync(data).ToListAsync();

        return ApiResult.Ok(result);
    }

    private async IAsyncEnumerable<DashboardInfoRow> MapAsync(List<DashboardRow> rows)
    {
        foreach (var row in rows)
        {
            var user = await _dataResolveManager.GetUserAsync(row.UserId.ToUlong());

            yield return new DashboardInfoRow
            {
                Result = GetText(row.Type),
                Name = FormatUser(user, row.UserId)
            };
        }
    }

    private string GetText(string id)
        => Texts[$"User/UserMeasures/{id}", ApiContext.Language];

    private string FormatUser(Data.Models.API.Users.User? user, string defaultValue)
    {
        if (user is null)
            return string.Format(GetText("UnknownUser"), defaultValue);

        var entity = new Database.Entity.User
        {
            Username = user.Username,
            GlobalAlias = user.GlobalAlias,
        };

        return $"User({user.Id}/{entity.GetDisplayName()})";
    }
}
