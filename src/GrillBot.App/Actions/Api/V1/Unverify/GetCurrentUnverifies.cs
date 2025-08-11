using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Unverify;
using System.Text.Json;
using UnverifyService;
using UnverifyService.Core.Enums;
using UnverifyService.Models.Request;
using UnverifyService.Models.Response.Logs.Detail;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetCurrentUnverifies(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    DataResolveManager _dataResolve
    ) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var request = new ActiveUnverifyListRequest();

        var unverifies = await _unverifyClient.ExecuteRequestAsync(async (client, ctx) =>
        {
            return ApiContext.IsPublic() ?
                await client.GetCurrentUserUnverifyListAsync(request, ctx.AuthorizationToken, ctx.CancellationToken) :
                await client.GetActiveUnverifyListAsync(request, ctx.CancellationToken);
        }, CancellationToken);

        var result = new List<UnverifyUserProfile>();
        foreach (var unverify in unverifies.Data)
        {
            var logDetail = await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.GetUnverifyLogDetailAsync(unverify.Id, ctx.CancellationToken),
                CancellationToken
            );

            if (logDetail is not null)
            {
                var profile = await CreateProfileAsync(logDetail);
                if (profile is not null)
                    result.Add(profile);
            }
        }

        return ApiResult.Ok(result);
    }

    private async Task<UnverifyUserProfile?> CreateProfileAsync(UnverifyLogDetail detail)
    {
        if (detail.Data is not JsonElement detailData)
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var unverifyDetailData = detail.OperationType == UnverifyOperationType.Unverify ? detailData.Deserialize<UnverityOperationDetailData>(options) : null;
        var selfUnverifyDetailData = detail.OperationType == UnverifyOperationType.SelfUnverify ? detailData.Deserialize<SelfUnverifyOperationDetailData>(options) : null;

        if (unverifyDetailData is null && selfUnverifyDetailData is null)
            return null;

        var startAt = (unverifyDetailData?.EndAtUtc ?? selfUnverifyDetailData?.EndAtUtc)!.Value.WithKind(DateTimeKind.Utc).ToLocalTime();
        var endAt = (unverifyDetailData?.EndAtUtc ?? selfUnverifyDetailData?.EndAtUtc)!.Value.WithKind(DateTimeKind.Utc).ToLocalTime();
        var guild = await _discordClient.GetGuildAsync(detail.GuildId.ToUlong(), options: new() { CancelToken = CancellationToken });

        if (guild is null)
            return null;

        var profile = new UnverifyUserProfile
        {
            End = endAt,
            ChannelsToKeep = (unverifyDetailData?.KeepedChannels ?? selfUnverifyDetailData?.KeepedChannels)?.Select(o => o.ChannelId.ToString())?.ToList() ?? [],
            ChannelsToRemove = (unverifyDetailData?.RemovedChannels ?? selfUnverifyDetailData?.RemovedChannels)?.Select(o => o.ChannelId.ToString())?.ToList() ?? [],
            EndTo = endAt - DateTime.Now,
            Guild = (await _dataResolve.GetGuildAsync(detail.GuildId.ToUlong(), CancellationToken))!,
            IsSelfUnverify = detail.OperationType == UnverifyOperationType.SelfUnverify,
            Reason = unverifyDetailData?.Reason ?? "",
            Start = startAt,
            User = (await _dataResolve.GetUserAsync(detail.ToUserId.ToUlong(), CancellationToken))!
        };

        foreach (var roleId in unverifyDetailData?.KeepedRoles ?? selfUnverifyDetailData?.KeepedRoles ?? [])
        {
            var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
            if (role is not null)
                profile.RolesToKeep.Add(role);
        }

        foreach (var roleId in unverifyDetailData?.RemovedRoles ?? selfUnverifyDetailData?.RemovedRoles ?? [])
        {
            var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
            if (role is not null)
                profile.RolesToRemove.Add(role);
        }

        return profile;
    }
}
