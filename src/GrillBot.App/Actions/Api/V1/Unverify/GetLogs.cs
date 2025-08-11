using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Enums;
using System.Text.Json;
using UnverifyService;
using UnverifyService.Core.Enums;
using UnverifyService.Models.Request.Logs;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetLogs(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    DataResolveManager _dataResolve
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (UnverifyLogParams)Parameters[0]!;
        var mutualGuilds = await GetMutualGuildsAsync();
        UpdatePublicAccess(parameters, mutualGuilds);

        var request = new UnverifyLogListRequest
        {
            CreatedFrom = parameters.Created?.From?.ToUniversalTime(),
            CreatedTo = parameters.Created?.To?.ToUniversalTime(),
            FromUserId = parameters.FromUserId,
            ToUserId = parameters.ToUserId,
            GuildId = parameters.GuildId,
            Operation = parameters.Operation switch
            {
                UnverifyOperation.Unverify => UnverifyOperationType.Unverify,
                UnverifyOperation.Selfunverify => UnverifyOperationType.SelfUnverify,
                UnverifyOperation.Autoremove => UnverifyOperationType.AutoRemove,
                UnverifyOperation.Remove => UnverifyOperationType.ManualRemove,
                UnverifyOperation.Update => UnverifyOperationType.Update,
                UnverifyOperation.Recover => UnverifyOperationType.Recovery,
                _ => null
            },
            Pagination = parameters.Pagination,
            Sort = new Core.Models.SortParameters
            {
                Descending = parameters.Sort.Descending,
                OrderBy = parameters.Sort.OrderBy
            }
        };

        var logs = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetUnverifyLogsAsync(request, ctx.CancellationToken),
            CancellationToken
        );

        var result = await PaginatedResponse<UnverifyLogItem>.CopyAndMapAsync(logs, MapItemAsync);
        return ApiResult.Ok(result);
    }

    private async Task<List<string>> GetMutualGuildsAsync()
    {
        if (!ApiContext.IsPublic())
            return [];

        var mutualGuilds = await _discordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        return mutualGuilds.ConvertAll(o => o.Id.ToString());
    }

    private void UpdatePublicAccess(UnverifyLogParams parameters, List<string> mutualGuilds)
    {
        if (!ApiContext.IsPublic()) return;

        parameters.FromUserId = null;
        parameters.ToUserId = ApiContext.GetUserId().ToString();
        if (!string.IsNullOrEmpty(parameters.GuildId) && !mutualGuilds.Contains(parameters.GuildId))
            parameters.GuildId = null;
    }

    private async Task<UnverifyLogItem> MapItemAsync(UnverifyService.Models.Response.Logs.UnverifyLogItem item)
    {
        var result = new UnverifyLogItem
        {
            CreatedAt = item.CreatedAtUtc.ToLocalTime(),
            FromUser = (await _dataResolve.GetGuildUserAsync(item.GuildId.ToUlong(), item.FromUserId.ToUlong(), CancellationToken))!,
            Guild = (await _dataResolve.GetGuildAsync(item.GuildId.ToUlong(), CancellationToken))!,
            Id = item.LogNumber,
            Operation = item.Type switch
            {
                UnverifyOperationType.Unverify => UnverifyOperation.Unverify,
                UnverifyOperationType.SelfUnverify => UnverifyOperation.Selfunverify,
                UnverifyOperationType.AutoRemove => UnverifyOperation.Autoremove,
                UnverifyOperationType.ManualRemove => UnverifyOperation.Remove,
                UnverifyOperationType.Update => UnverifyOperation.Update,
                UnverifyOperationType.Recovery => UnverifyOperation.Recover,
                _ => throw new ArgumentOutOfRangeException($"Invalid operation type ({item.Type})", nameof(item.Type))
            },
            ToUser = (await _dataResolve.GetGuildUserAsync(item.GuildId.ToUlong(), item.ToUserId.ToUlong(), CancellationToken))!
        };

        var logDetail = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetUnverifyLogDetailAsync(item.Id, ctx.CancellationToken),
            CancellationToken
        );

        if (logDetail is null || logDetail.Data is not JsonElement element)
            return result;

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        switch (item.Type)
        {
            case UnverifyOperationType.AutoRemove:
                {
                    var jsonData = element.Deserialize<UnverifyService.Models.Response.Logs.Detail.AutoRemoveOperationDetailData>(serializerOptions)!;

                    result.RemoveData = new UnverifyLogRemove
                    {
                        Language = jsonData.Language,
                        ReturnedChannelIds = [.. jsonData.ReturnedChannels.Select(o => o.ChannelId.ToString())]
                    };

                    foreach (var roleId in jsonData.ReturnedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.RemoveData.ReturnedRoles.Add(role);
                    }
                }
                break;
            case UnverifyOperationType.ManualRemove:
                {
                    var jsonData = element.Deserialize<UnverifyService.Models.Response.Logs.Detail.ManualRemoveOperationDetailData>(serializerOptions)!;

                    result.RemoveData = new UnverifyLogRemove
                    {
                        Force = jsonData.IsForceRemove,
                        FromWeb = jsonData.IsFromWeb,
                        Language = jsonData.Language,
                        ReturnedChannelIds = [.. jsonData.ReturnedChannels.Select(o => o.ChannelId.ToString())]
                    };

                    foreach (var roleId in jsonData.ReturnedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.RemoveData.ReturnedRoles.Add(role);
                    }
                }
                break;
            case UnverifyOperationType.Recovery:
                {
                    var jsonData = element.Deserialize<UnverifyService.Models.Response.Logs.Detail.RecoveryOperationDetailData>(serializerOptions)!;

                    result.RemoveData = new UnverifyLogRemove
                    {
                        ReturnedChannelIds = [.. jsonData.ReturnedChannels.Select(o => o.ChannelId.ToString())],
                    };

                    foreach (var roleId in jsonData.ReturnedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.RemoveData.ReturnedRoles.Add(role);
                    }
                }
                break;
            case UnverifyOperationType.SelfUnverify:
                {
                    var jsonData = element.Deserialize<UnverifyService.Models.Response.Logs.Detail.SelfUnverifyOperationDetailData>(serializerOptions)!;

                    result.SetData = new UnverifyLogSet
                    {
                        ChannelIdsToKeep = [.. jsonData.KeepedChannels.Select(o => o.ChannelId.ToString())],
                        ChannelIdsToRemove = [.. jsonData.RemovedChannels.Select(o => o.ChannelId.ToString())],
                        End = jsonData.EndAtUtc.ToLocalTime(),
                        IsSelfUnverify = true,
                        Language = jsonData.Language,
                        Reason = null,
                        Start = jsonData.StartAtUtc.ToLocalTime()
                    };

                    foreach (var roleId in jsonData.KeepedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.SetData.RolesToKeep.Add(role);
                    }

                    foreach (var roleId in jsonData.RemovedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.SetData.RolesToRemove.Add(role);
                    }
                }
                break;
            case UnverifyOperationType.Unverify:
                {
                    var jsonData = element.Deserialize<UnverifyService.Models.Response.Logs.Detail.UnverityOperationDetailData>(serializerOptions)!;

                    result.SetData = new UnverifyLogSet
                    {
                        ChannelIdsToKeep = [.. jsonData.KeepedChannels.Select(o => o.ChannelId.ToString())],
                        ChannelIdsToRemove = [.. jsonData.RemovedChannels.Select(o => o.ChannelId.ToString())],
                        End = jsonData.EndAtUtc.ToLocalTime(),
                        Language = jsonData.Language,
                        Reason = jsonData.Reason,
                        Start = jsonData.StartAtUtc.ToLocalTime()
                    };

                    foreach (var roleId in jsonData.KeepedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.SetData.RolesToKeep.Add(role);
                    }

                    foreach (var roleId in jsonData.RemovedRoles)
                    {
                        var role = await _dataResolve.GetRoleAsync(roleId.ToUlong(), CancellationToken);
                        if (role is not null)
                            result.SetData.RolesToRemove.Add(role);
                    }
                }
                break;
            case UnverifyOperationType.Update:
                {
                    var jsonData = element.Deserialize<UnverifyService.Models.Response.Logs.Detail.UpdateOperationDetailData>(serializerOptions)!;

                    result.UpdateData = new UnverifyLogUpdate
                    {
                        End = jsonData.NewEndAtUtc.ToLocalTime(),
                        Start = jsonData.NewStartAtUtc.ToLocalTime(),
                        Reason = jsonData.Reason
                    };
                }
                break;
            default:
                throw new ArgumentOutOfRangeException($"Invalid operation type ({item.Type})", nameof(item.Type));
        }

        return result;
    }
}
