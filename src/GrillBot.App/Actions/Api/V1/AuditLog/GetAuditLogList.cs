using System.Text.Json;
using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.Search;
using GrillBot.Core.Services.FileService;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Preview;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using File = GrillBot.Data.Models.API.AuditLog.File;
using SearchModels = GrillBot.Core.Services.AuditLog.Models.Response.Search;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IFileServiceClient FileServiceClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private IDiscordClient DiscordClient { get; }

    public GetAuditLogList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IFileServiceClient fileServiceClient, IAuditLogServiceClient auditLogServiceClient,
        IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        FileServiceClient = fileServiceClient;
        AuditLogServiceClient = auditLogServiceClient;
        DiscordClient = discordClient;
    }

    public async Task<PaginatedResponse<LogListItem>> ProcessAsync(SearchRequest request)
    {
        FixDateTimes(request);

        var response = await AuditLogServiceClient.SearchItemsAsync(request);
        if (response.ValidationErrors is not null)
            throw CreateValidationExceptions(response.ValidationErrors);

        await using var repository = DatabaseBuilder.CreateRepository();
        return await PaginatedResponse<LogListItem>.CopyAndMapAsync(response.Response!, async entity => await MapListItemAsync(repository, entity));
    }

    private static void FixDateTimes(SearchRequest request)
    {
        request.CreatedFrom = FixDateTime(request.CreatedFrom);
        request.CreatedTo = FixDateTime(request.CreatedTo);
    }

    private static DateTime? FixDateTime(DateTime? dateTime)
    {
        if (dateTime is null)
            return null;
        if (dateTime.Value.Kind == DateTimeKind.Utc)
            return dateTime;

        var val = dateTime.Value;
        return new DateTime(val.Year, val.Month, val.Day, val.Hour, val.Minute, val.Second, val.Millisecond, DateTimeKind.Local).ToUniversalTime();
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

    private async Task<LogListItem> MapListItemAsync(GrillBotRepository repository, SearchModels.LogListItem item)
    {
        var result = new LogListItem
        {
            Type = item.Type,
            CreatedAt = item.CreatedAt.ToLocalTime(),
            IsDetailAvailable = item.IsDetailAvailable,
            Id = item.Id
        };

        foreach (var file in item.Files)
            result.Files.Add(await ConvertFileAsync(file));

        if (!string.IsNullOrEmpty(item.GuildId))
        {
            var guild = await repository.Guild.FindGuildByIdAsync(item.GuildId.ToUlong(), true);
            if (guild is not null)
            {
                result.Guild = Mapper.Map<Data.Models.API.Guilds.Guild>(guild);

                if (!string.IsNullOrEmpty(item.ChannelId))
                {
                    var channel = await repository.Channel.FindChannelByIdAsync(item.ChannelId.ToUlong(), guild.Id.ToUlong(), true, includeDeleted: true);
                    if (channel is not null)
                        result.Channel = Mapper.Map<Data.Models.API.Channels.Channel>(channel);
                }
            }
        }

        if (!string.IsNullOrEmpty(item.UserId))
        {
            var user = await repository.User.FindUserByIdAsync(item.UserId.ToUlong(), disableTracking: true);
            if (user is not null)
                result.User = Mapper.Map<Data.Models.API.Users.User>(user);
        }

        result.Preview = await MapPreviewAsync(repository, item);
        return result;
    }

    private async Task<object?> MapPreviewAsync(GrillBotRepository repository, SearchModels.LogListItem item)
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
                    var role = preview.TargetType == PermissionTarget.Role ? await DiscordClient.FindRoleAsync(preview.TargetId.ToUlong()) : null;
                    var user = preview.TargetType == PermissionTarget.User ? await repository.User.FindUserByIdAsync(preview.TargetId.ToUlong()) : null;

                    return new OverwritePreview
                    {
                        Role = Mapper.Map<Data.Models.API.Role>(role),
                        User = Mapper.Map<Data.Models.API.Users.User>(user),
                        Allow = preview.Allow,
                        Deny = preview.Deny
                    };
                }
            case LogType.OverwriteUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.OverwriteUpdatedPreview>(options)!;
                    var role = preview.TargetType == PermissionTarget.Role ? await DiscordClient.FindRoleAsync(preview.TargetId.ToUlong()) : null;
                    var user = preview.TargetType == PermissionTarget.User ? await repository.User.FindUserByIdAsync(preview.TargetId.ToUlong(), disableTracking: true) : null;

                    return new OverwriteUpdatedPreview
                    {
                        Role = Mapper.Map<Data.Models.API.Role>(role),
                        User = Mapper.Map<Data.Models.API.Users.User>(user),
                    };
                }
            case LogType.Unban:
                {
                    var preview = jsonElement.Deserialize<SearchModels.UnbanPreview>(options)!;
                    var user = await repository.User.FindUserByIdAsync(preview.UserId.ToUlong(), disableTracking: true);

                    return new UnbanPreview
                    {
                        User = Mapper.Map<Data.Models.API.Users.User>(user)
                    };
                }
            case LogType.MemberUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.MemberUpdatedPreview>(options)!;
                    var user = await repository.User.FindUserByIdAsync(preview.UserId.ToUlong(), disableTracking: true);

                    return new MemberUpdatedPreview
                    {
                        User = Mapper.Map<Data.Models.API.Users.User>(user),
                        SelfUnverifyMinimalTimeChange = preview.SelfUnverifyMinimalTimeChange,
                        FlagsChanged = preview.FlagsChanged,
                        NicknameChanged = preview.NicknameChanged,
                        VoiceMuteChanged = preview.VoiceMuteChanged
                    };
                }
            case LogType.MemberRoleUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.MemberRoleUpdatedPreview>(options)!;
                    var user = await repository.User.FindUserByIdAsync(preview.UserId.ToUlong(), disableTracking: true);

                    return new MemberRoleUpdatedPreview
                    {
                        ModifiedRoles = preview.ModifiedRoles,
                        User = Mapper.Map<Data.Models.API.Users.User>(user)
                    };
                }
            case LogType.GuildUpdated:
                return jsonElement.Deserialize<SearchModels.GuildUpdatedPreview>(options);
            case LogType.UserLeft:
                {
                    var preview = jsonElement.Deserialize<SearchModels.UserLeftPreview>(options)!;
                    var user = await repository.User.FindUserByIdAsync(preview.UserId.ToUlong(), disableTracking: true);

                    return new UserLeftPreview
                    {
                        User = Mapper.Map<Data.Models.API.Users.User>(user),
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
                    var user = await repository.User.FindUserByIdAsync(preview.AuthorId.ToUlong(), disableTracking: true);

                    return new MessageDeletedPreview
                    {
                        User = Mapper.Map<Data.Models.API.Users.User>(user),
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

    private async Task<File> ConvertFileAsync(SearchModels.File file)
    {
        var link = await FileServiceClient.GenerateLinkAsync(file.Filename);

        return new File
        {
            Filename = file.Filename,
            Link = link!,
            Size = file.Size
        };
    }
}
