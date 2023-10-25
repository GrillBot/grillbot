using System.Text.Json;
using AutoMapper;
using GrillBot.App.Helpers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.FileStorage;
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
using Microsoft.Extensions.Azure;
using File = GrillBot.Data.Models.API.AuditLog.File;
using SearchModels = GrillBot.Core.Services.AuditLog.Models.Response.Search;

namespace GrillBot.App.Actions.Api.V1.AuditLog;

public class GetAuditLogList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private IDiscordClient DiscordClient { get; }
    private BlobManagerFactoryHelper BlobManagerFactoryHelper { get; }

    private Dictionary<string, Database.Entity.User> CachedUsers { get; } = new();
    private Dictionary<string, Database.Entity.Guild> CachedGuilds { get; } = new();
    private Dictionary<string, Dictionary<string, Database.Entity.GuildChannel>> CachedChannels { get; } = new(); // Dictionary<GuildId, Dictionary<ChannelId, Channel>>

    private BlobManager BlobManager { get; set; } = null!;
    private BlobManager LegacyBlobManager { get; set; } = null!;

    public GetAuditLogList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IAuditLogServiceClient auditLogServiceClient, IDiscordClient discordClient,
        BlobManagerFactoryHelper blobManagerFactoryHelper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        AuditLogServiceClient = auditLogServiceClient;
        DiscordClient = discordClient;
        BlobManagerFactoryHelper = blobManagerFactoryHelper;
    }

    public async Task<PaginatedResponse<LogListItem>> ProcessAsync(SearchRequest request)
    {
        FixDateTimes(request);

        var response = await AuditLogServiceClient.SearchItemsAsync(request);
        if (response.ValidationErrors is not null)
            throw CreateValidationExceptions(response.ValidationErrors);

        if (response.Response!.Data.Exists(o => o.Files.Count > 0))
        {
            BlobManager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
            LegacyBlobManager = await BlobManagerFactoryHelper.CreateLegacyAsync();
        }

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

        return dateTime.Value.WithKind(DateTimeKind.Local).ToUniversalTime();
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
            Id = item.Id,
            Files = item.Files.ConvertAll(o => ConvertFile(o, item))
        };

        if (!string.IsNullOrEmpty(item.GuildId))
        {
            var guild = await ResolveGuildAsync(repository, item.GuildId);
            if (guild is not null)
            {
                result.Guild = Mapper.Map<Data.Models.API.Guilds.Guild>(guild);

                if (!string.IsNullOrEmpty(item.ChannelId))
                {
                    var channel = await ResolveChannelAsync(repository, item.GuildId, item.ChannelId);
                    if (channel is not null)
                        result.Channel = Mapper.Map<Data.Models.API.Channels.Channel>(channel);
                }
            }
        }

        if (!string.IsNullOrEmpty(item.UserId))
        {
            var user = await ResolveUserAsync(repository, item.UserId);
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
                    var user = preview.TargetType == PermissionTarget.User ? await ResolveUserAsync(repository, preview.TargetId) : null;

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
                    var user = preview.TargetType == PermissionTarget.User ? await ResolveUserAsync(repository, preview.TargetId) : null;

                    return new OverwriteUpdatedPreview
                    {
                        Role = Mapper.Map<Data.Models.API.Role>(role),
                        User = Mapper.Map<Data.Models.API.Users.User>(user),
                    };
                }
            case LogType.Unban:
                {
                    var preview = jsonElement.Deserialize<SearchModels.UnbanPreview>(options)!;
                    var user = await ResolveUserAsync(repository, preview.UserId);

                    return new UnbanPreview
                    {
                        User = Mapper.Map<Data.Models.API.Users.User>(user)
                    };
                }
            case LogType.MemberUpdated:
                {
                    var preview = jsonElement.Deserialize<SearchModels.MemberUpdatedPreview>(options)!;
                    var user = await ResolveUserAsync(repository, preview.UserId);

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
                    var user = await ResolveUserAsync(repository, preview.UserId);

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
                    var user = await ResolveUserAsync(repository, preview.UserId);

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
                    var user = await ResolveUserAsync(repository, preview.AuthorId);

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

    private async Task<Database.Entity.User?> ResolveUserAsync(GrillBotRepository repository, string userId)
    {
        if (CachedUsers.TryGetValue(userId, out var user))
            return user;

        user = await repository.User.FindUserByIdAsync(userId.ToUlong(), disableTracking: true);
        if (user is null)
            return null;

        CachedUsers.Add(user.Id, user);
        return user;
    }

    private async Task<Database.Entity.Guild?> ResolveGuildAsync(GrillBotRepository repository, string guildId)
    {
        if (CachedGuilds.TryGetValue(guildId, out var guild))
            return guild;

        guild = await repository.Guild.FindGuildByIdAsync(guildId.ToUlong(), true);
        if (guild is null)
            return null;

        CachedGuilds.Add(guild.Id, guild);
        return guild;
    }

    private async Task<Database.Entity.GuildChannel?> ResolveChannelAsync(GrillBotRepository repository, string guildId, string channelId)
    {
        if (CachedChannels.TryGetValue(guildId, out var guildChannels) && guildChannels.TryGetValue(channelId, out var guildChannel))
            return guildChannel;

        guildChannel = await repository.Channel.FindChannelByIdAsync(channelId.ToUlong(), guildId.ToUlong(), true, includeDeleted: true);
        if (guildChannel is null)
            return null;

        if (!CachedChannels.ContainsKey(guildId))
            CachedChannels.Add(guildId, new Dictionary<string, Database.Entity.GuildChannel>());

        CachedChannels[guildId].Add(channelId, guildChannel);
        return guildChannel;
    }
}
