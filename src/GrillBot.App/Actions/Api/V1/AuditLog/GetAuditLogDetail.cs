using System.Text.Json;
using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models.Response.Detail;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogDetail : ApiAction
{
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }

    public GetAuditLogDetail(ApiRequestContext apiContext, IAuditLogServiceClient auditLogServiceClient, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient,
        IMapper mapper) : base(apiContext)
    {
        AuditLogServiceClient = auditLogServiceClient;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Mapper = mapper;
    }

    public async Task<Detail?> ProcessAsync(Guid id)
    {
        var detail = await AuditLogServiceClient.DetailAsync(id);
        if (detail?.Data is not JsonElement jsonElement)
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        await using var repository = DatabaseBuilder.CreateRepository();

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
                var role = overwriteUpdated.TargetType == PermissionTarget.Role ? await DiscordClient.FindRoleAsync(overwriteUpdated.TargetId.ToUlong()) : null;
                var user = overwriteUpdated.TargetType == PermissionTarget.User ? await repository.User.FindUserByIdAsync(overwriteUpdated.TargetId.ToUlong()) : null;

                detail.Data = new Data.Models.API.AuditLog.Detail.OverwriteUpdatedDetail
                {
                    User = Mapper.Map<Data.Models.API.Users.User>(user),
                    Role = Mapper.Map<Data.Models.API.Role>(role),
                    Allow = overwriteUpdated.Allow,
                    Deny = overwriteUpdated.Deny
                };
                break;
            }
            case LogType.MemberUpdated:
            {
                var memberUpdated = jsonElement.Deserialize<MemberUpdatedDetail>(options)!;
                var user = await repository.User.FindUserByIdAsync(memberUpdated.UserId.ToUlong());

                detail.Data = new Data.Models.API.AuditLog.Detail.MemberUpdatedDetail
                {
                    User = Mapper.Map<Data.Models.API.Users.User>(user),
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
                var author = await repository.User.FindUserByIdAsync(messageDeleted.AuthorId.ToUlong());

                detail.Data = new Data.Models.API.AuditLog.Detail.MessageDeletedDetail
                {
                    Author = Mapper.Map<Data.Models.API.Users.User>(author),
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
                var startUser = string.IsNullOrEmpty(jobCompleted.StartUserId) ? null : await repository.User.FindUserByIdAsync(jobCompleted.StartUserId.ToUlong());

                detail.Data = new Data.Models.API.AuditLog.Detail.JobExecutionDetail
                {
                    Result = jobCompleted.Result,
                    EndAt = jobCompleted.EndAt.ToLocalTime(),
                    JobName = jobCompleted.JobName,
                    StartAt = jobCompleted.StartAt.ToLocalTime(),
                    StartUser = startUser is null ? null : Mapper.Map<Data.Models.API.Users.User>(startUser),
                    WasError = jobCompleted.WasError
                };
                break;
            }
            case LogType.Api:
                detail.Data = jsonElement.Deserialize<ApiRequestDetail>(options);
                break;
            case LogType.ThreadUpdated:
                detail.Data = jsonElement.Deserialize<ThreadUpdatedDetail>(options);
                break;
        }

        return detail;
    }
}
