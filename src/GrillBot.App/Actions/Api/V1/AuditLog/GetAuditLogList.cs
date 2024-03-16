using System.Text.Json;
using GrillBot.App.Helpers;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.Search;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Preview;
using Microsoft.AspNetCore.Mvc;
using File = GrillBot.Data.Models.API.AuditLog.File;
using SearchModels = GrillBot.Core.Services.AuditLog.Models.Response.Search;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogList : ApiAction
{
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private BlobManagerFactoryHelper BlobManagerFactoryHelper { get; }

    private readonly DataResolveManager _dataResolveManager;

    private BlobManager BlobManager { get; set; } = null!;
    private BlobManager LegacyBlobManager { get; set; } = null!;

    public GetAuditLogList(ApiRequestContext apiContext, IAuditLogServiceClient auditLogServiceClient, BlobManagerFactoryHelper blobManagerFactoryHelper,
        DataResolveManager dataResolveManager) : base(apiContext)
    {
        AuditLogServiceClient = auditLogServiceClient;
        BlobManagerFactoryHelper = blobManagerFactoryHelper;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (SearchRequest)Parameters[0]!;

        request.CreatedFrom = Common.Extensions.DateTimeExtensions.ConvertKindToUtc(request.CreatedFrom, DateTimeKind.Local);
        request.CreatedTo = Common.Extensions.DateTimeExtensions.ConvertKindToUtc(request.CreatedTo, DateTimeKind.Local);

        var response = await AuditLogServiceClient.SearchItemsAsync(request);
        if (response.ValidationErrors is not null)
            throw CreateValidationExceptions(response.ValidationErrors);

        if (response.Response!.Data.Exists(o => o.Files.Count > 0))
        {
            BlobManager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
            LegacyBlobManager = await BlobManagerFactoryHelper.CreateLegacyAsync();
        }

        var result = await PaginatedResponse<LogListItem>.CopyAndMapAsync(response.Response!, MapListItemAsync);

        return ApiResult.Ok(result);
    }

    private static AggregateException CreateValidationExceptions(ValidationProblemDetails validationProblemDetails)
    {
        var exceptions = new List<Exception>();
        foreach (var error in validationProblemDetails.Errors)
        {
            exceptions.AddRange(
                error.Value
                    .Select(msg => new ValidationResult(msg, new[] { error.Key }))
                    .Select(validationResult => new ValidationException(validationResult, null, null))
            );
        }

        return new AggregateException(exceptions.ToArray());
    }

    private async Task<LogListItem> MapListItemAsync(SearchModels.LogListItem item)
    {
        var result = new LogListItem
        {
            Type = item.Type,
            CreatedAt = item.CreatedAt.ToLocalTime(),
            IsDetailAvailable = item.IsDetailAvailable,
            Id = item.Id,
            Files = item.Files.ConvertAll(o => ConvertFile(o, item))
        };

        if (!string.IsNullOrEmpty(item.GuildId))
        {
            result.Guild = await _dataResolveManager.GetGuildAsync(item.GuildId.ToUlong());

            if (result.Guild is not null && !string.IsNullOrEmpty(item.ChannelId))
                result.Channel = await _dataResolveManager.GetChannelAsync(item.GuildId.ToUlong(), item.ChannelId.ToUlong());
        }

        if (!string.IsNullOrEmpty(item.UserId))
            result.User = await _dataResolveManager.GetUserAsync(item.UserId.ToUlong());

        result.Preview = await MapPreviewAsync(item);
        return result;
    }

    private async Task<object?> MapPreviewAsync(SearchModels.LogListItem item)
    {
        if (item.Preview is not JsonElement jsonElement)
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        switch (item.Type)
        {
            case LogType.Info or LogType.Warning or LogType.Error:
                return jsonElement.Deserialize<SearchModels.TextPreview>(options);
            case LogType.ChannelCreated or LogType.ChannelDeleted:
                return jsonElement.Deserialize<SearchModels.ChannelPreview>(options);
            case LogType.ChannelUpdated:
                return jsonElement.Deserialize<SearchModels.ChannelUpdatedPreview>(options);
            case LogType.EmoteDeleted:
                return jsonElement.Deserialize<SearchModels.EmoteDeletedPreview>(options);
            case LogType.OverwriteCreated or LogType.OverwriteDeleted:
                {
                    var preview = jsonElement.Deserialize<SearchModels.OverwritePreview>(options)!;
                    var previewData = new OverwritePreview
                    {
                        Allow = preview.Allow,
                        Deny = preview.Deny
                    };

                    if (preview.TargetType is PermissionTarget.Role)
                        previewData.Role = await _dataResolveManager.GetRoleAsync(preview.TargetId.ToUlong());
                    else if (preview.TargetType is PermissionTarget.User)
                        previewData.User = await _dataResolveManager.GetUserAsync(preview.TargetId.ToUlong());

                    return previewData;
                }
            case LogType.OverwriteUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.OverwriteUpdatedPreview>(options)!;
                    var previewData = new OverwriteUpdatedPreview();

                    if (preview.TargetType is PermissionTarget.Role)
                        previewData.Role = await _dataResolveManager.GetRoleAsync(preview.TargetId.ToUlong());
                    else if (preview.TargetType is PermissionTarget.User)
                        previewData.User = await _dataResolveManager.GetUserAsync(preview.TargetId.ToUlong());

                    return previewData;
                }
            case LogType.Unban:
                {
                    var preview = jsonElement.Deserialize<SearchModels.UnbanPreview>(options)!;

                    return new UnbanPreview
                    {
                        User = (await _dataResolveManager.GetUserAsync(preview.UserId.ToUlong()))!
                    };
                }
            case LogType.MemberUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.MemberUpdatedPreview>(options)!;

                    return new MemberUpdatedPreview
                    {
                        User = (await _dataResolveManager.GetUserAsync(preview.UserId.ToUlong()))!,
                        SelfUnverifyMinimalTimeChange = preview.SelfUnverifyMinimalTimeChange,
                        FlagsChanged = preview.FlagsChanged,
                        NicknameChanged = preview.NicknameChanged,
                        VoiceMuteChanged = preview.VoiceMuteChanged
                    };
                }
            case LogType.MemberRoleUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.MemberRoleUpdatedPreview>(options)!;

                    return new MemberRoleUpdatedPreview
                    {
                        ModifiedRoles = preview.ModifiedRoles,
                        User = (await _dataResolveManager.GetUserAsync(preview.UserId.ToUlong()))!
                    };
                }
            case LogType.GuildUpdated:
                return jsonElement.Deserialize<SearchModels.GuildUpdatedPreview>(options);
            case LogType.UserLeft:
                {
                    var preview = jsonElement.Deserialize<SearchModels.UserLeftPreview>(options)!;

                    return new UserLeftPreview
                    {
                        User = (await _dataResolveManager.GetUserAsync(preview.UserId.ToUlong()))!,
                        BanReason = preview.BanReason,
                        IsBan = preview.IsBan,
                        MemberCount = preview.MemberCount
                    };
                }
            case LogType.UserJoined:
                return jsonElement.Deserialize<SearchModels.UserJoinedPreview>(options);
            case LogType.MessageEdited:
                return jsonElement.Deserialize<SearchModels.MessageEditedPreview>(options);
            case LogType.MessageDeleted:
                {
                    var preview = jsonElement.Deserialize<SearchModels.MessageDeletedPreview>(options)!;

                    return new MessageDeletedPreview
                    {
                        User = (await _dataResolveManager.GetUserAsync(preview.AuthorId.ToUlong()))!,
                        Content = preview.Content,
                        Embeds = preview.Embeds,
                        MessageCreatedAt = preview.MessageCreatedAt.ToLocalTime()
                    };
                }
            case LogType.InteractionCommand:
                return jsonElement.Deserialize<SearchModels.InteractionCommandPreview>(options);
            case LogType.ThreadDeleted:
                return jsonElement.Deserialize<SearchModels.ThreadDeletedPreview>(options);
            case LogType.JobCompleted:
                return jsonElement.Deserialize<SearchModels.JobPreview>(options);
            case LogType.Api:
                return jsonElement.Deserialize<SearchModels.ApiPreview>(options);
            case LogType.ThreadUpdated:
                return jsonElement.Deserialize<SearchModels.ThreadUpdatedPreview>(options);
            case LogType.RoleDeleted:
                return jsonElement.Deserialize<SearchModels.RoleDeletedPreview>(options);
        }

        return null;
    }

    private File ConvertFile(SearchModels.File file, SearchModels.LogListItem item)
    {
        // TODO Hack until all old files has been deleted.
        var migrationDate = new DateTime(2023, 10, 25, 12, 00, 00, DateTimeKind.Utc);
        var usedManager = item.CreatedAt >= migrationDate ? BlobManager : LegacyBlobManager;
        var link = usedManager.GenerateSasLink(file.Filename, 1);

        return new File
        {
            Filename = file.Filename,
            Link = link ?? "about:blank",
            Size = file.Size
        };
    }
}
