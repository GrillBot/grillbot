using Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetUserMeasuresDashboard : ApiAction
{
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public GetUserMeasuresDashboard(ApiRequestContext apiContext, IAuditLogServiceClient auditLogServiceClient, GrillBotDatabaseBuilder databaseBuilder,
        ITextsManager texts) : base(apiContext)
    {
        AuditLogServiceClient = auditLogServiceClient;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var memberWarnings = await AuditLogServiceClient.GetMemberWarningDashboardAsync();
        var unverifyLogs = await GetUnverifyLogsAsync(repository);
        var result = await MergeAndMapAsync(repository, memberWarnings, unverifyLogs);

        return ApiResult.Ok(result);
    }

    private static async Task<List<UnverifyLog>> GetUnverifyLogsAsync(GrillBotRepository repository)
    {
        var logRequest = new UnverifyLogParams
        {
            Operation = UnverifyOperation.Unverify,
            Pagination =
            {
                Page = 0,
                PageSize = 10
            },
            Sort =
            {
                Descending = true,
                OrderBy = "CreatedAt"
            }
        };

        var result = await repository.Unverify.GetLogsAsync(logRequest, logRequest.Pagination, new List<string>());
        return result.Data;
    }

    private async Task<List<DashboardInfoRow>> MergeAndMapAsync(GrillBotRepository repository, List<DashboardInfoRow> memberWarnings, List<UnverifyLog> unverifyLogs)
    {
        var userIds = memberWarnings
            .Select(o => o.Name)
            .Concat(unverifyLogs.Select(o => o.ToUserId))
            .Distinct()
            .ToList();

        var users = await repository.User.GetUsersByIdsAsync(userIds);
        var result = new List<DashboardInfoRow>();

        foreach (var item in memberWarnings)
        {
            var user = users.Find(o => o.Id == item.Name);

            item.Name = FormatUser(user, item.Name);
            item.Result = $"{item.Result}/{GetText("MemberWarning")}";

            result.Add(item);
        }

        foreach (var item in unverifyLogs)
        {
            var user = users.Find(o => o.Id == item.ToUserId);

            result.Add(new DashboardInfoRow
            {
                Name = FormatUser(user, item.ToUserId),
                Result = $"{item.CreatedAt:o}/{GetText("Unverify")}"
            });
        }

        result = result
            .OrderByDescending(o => DateTime.ParseExact(o.Result.Split('/')[0].Trim(), "o", CultureInfo.InvariantCulture))
            .Take(10)
            .ToList();

        foreach (var item in result)
            item.Result = item.Result.Split('/', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[1].Trim();

        return result;
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
