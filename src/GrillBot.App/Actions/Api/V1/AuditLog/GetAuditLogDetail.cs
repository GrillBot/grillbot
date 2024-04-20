using System.Text.Json;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Response.Detail;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogDetail : ApiAction
{
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    private readonly DataResolveManager _dataResolve;

    public GetAuditLogDetail(ApiRequestContext apiContext, IAuditLogServiceClient auditLogServiceClient, DataResolveManager dataResolve) : base(apiContext)
    {
        AuditLogServiceClient = auditLogServiceClient;
        _dataResolve = dataResolve;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (Guid)Parameters[0]!;
        var detail = await AuditLogServiceClient.GetDetailAsync(id);
        if (detail?.Data is not JsonElement jsonElement)
            return ApiResult.NotFound();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        switch (detail.Type)
        {
            case LogType.Info or LogType.Warning or LogType.Error:
                detail.Data = jsonElement.Deserialize<MessageDetail>(options);
                break;
            case LogType.ChannelUpdated:
                detail.Data = jsonElement.Deserialize<ChannelUpdatedDetail>(options);
                break;
            case LogType.OverwriteUpdated:
                {
                    var overwriteUpdated = jsonElement.Deserialize<OverwriteUpdatedDetail>(options)!;

                    var detailData = new Data.Models.API.AuditLog.Detail.OverwriteUpdatedDetail
                    {
                        Allow = overwriteUpdated.Allow,
                        Deny = overwriteUpdated.Deny
                    };

                    if (overwriteUpdated.TargetType is PermissionTarget.Role)
                        detailData.Role = await _dataResolve.GetRoleAsync(overwriteUpdated.TargetId.ToUlong());
                    else if (overwriteUpdated.TargetType is PermissionTarget.User)
                        detailData.User = await _dataResolve.GetUserAsync(overwriteUpdated.TargetId.ToUlong());

                    detail.Data = detailData;
                    break;
                }
            case LogType.MemberUpdated:
                {
                    var memberUpdated = jsonElement.Deserialize<MemberUpdatedDetail>(options)!;

                    detail.Data = new Data.Models.API.AuditLog.Detail.MemberUpdatedDetail
                    {
                        User = (await _dataResolve.GetUserAsync(memberUpdated.UserId.ToUlong()))!,
                        Flags = memberUpdated.Flags,
                        Nickname = memberUpdated.Nickname,
                        IsDeaf = memberUpdated.IsDeaf,
                        IsMuted = memberUpdated.IsMuted,
                        SelfUnverifyMinimalTime = memberUpdated.SelfUnverifyMinimalTime
                    };
                    break;
                }
            case LogType.GuildUpdated:
                detail.Data = jsonElement.Deserialize<GuildUpdatedDetail>(options);
                break;
            case LogType.MessageDeleted:
                {
                    var messageDeleted = jsonElement.Deserialize<MessageDeletedDetail>(options)!;

                    detail.Data = new Data.Models.API.AuditLog.Detail.MessageDeletedDetail
                    {
                        Author = (await _dataResolve.GetUserAsync(messageDeleted.AuthorId.ToUlong()))!,
                        Content = messageDeleted.Content,
                        Embeds = messageDeleted.Embeds,
                        MessageCreatedAt = messageDeleted.MessageCreatedAt.ToLocalTime()
                    };
                    break;
                }
            case LogType.InteractionCommand:
                detail.Data = jsonElement.Deserialize<InteractionCommandDetail>(options);
                break;
            case LogType.ThreadDeleted:
                detail.Data = jsonElement.Deserialize<ThreadDeletedDetail>(options);
                break;
            case LogType.JobCompleted:
                {
                    var jobCompleted = jsonElement.Deserialize<JobExecutionDetail>(options)!;

                    var detailData = new Data.Models.API.AuditLog.Detail.JobExecutionDetail
                    {
                        Result = jobCompleted.Result,
                        EndAt = jobCompleted.EndAt.ToLocalTime(),
                        JobName = jobCompleted.JobName,
                        StartAt = jobCompleted.StartAt.ToLocalTime(),
                        WasError = jobCompleted.WasError
                    };

                    if (!string.IsNullOrEmpty(jobCompleted.StartUserId))
                        detailData.StartUser = await _dataResolve.GetUserAsync(jobCompleted.StartUserId.ToUlong());

                    detail.Data = detailData;
                    break;
                }
            case LogType.Api:
                detail.Data = jsonElement.Deserialize<ApiRequestDetail>(options);
                break;
            case LogType.ThreadUpdated:
                detail.Data = jsonElement.Deserialize<ThreadUpdatedDetail>(options);
                break;
            case LogType.RoleDeleted:
                detail.Data = jsonElement.Deserialize<RoleDeletedDetail>(options);
                break;
        }

        return ApiResult.Ok(detail);
    }
}
