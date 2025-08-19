using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Database.Enums.Internal;
using GrillBot.Core.Infrastructure.Actions;
using ApiModels = GrillBot.Data.Models.API;
using Entity = GrillBot.Database.Entity;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Data.Enums;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Data.Extensions.Services;
using GrillBot.Core.Services.UserMeasures.Models.Measures;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.InviteService;
using GrillBot.Core.Services.InviteService.Models.Request;
using UserManagementService;
using UserManagementService.Models.Response;
using GrillBot.Core.Services.Common.Exceptions;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserDetail(
    ApiRequestContext apiContext,
    GrillBotDatabaseBuilder _databaseBuilder,
    IMapper _mapper,
    IDiscordClient _discordClient,
    ITextsManager _texts,
    IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient,
    IServiceClientExecutor<IUserMeasuresServiceClient> _userMeasuresService,
    DataResolveManager _dataResolveManager,
    IServiceClientExecutor<IEmoteServiceClient> _emoteServiceClient,
    IServiceClientExecutor<IInviteServiceClient> _inviteServiceClient,
    IServiceClientExecutor<IUserManagementServiceClient> _userManagement
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var userId = (ulong?)Parameters.ElementAtOrDefault(0);
        userId ??= ApiContext.GetUserId();

        var result = await ProcessAsync(userId.Value);
        if (ApiContext.IsPublic())
            result.RemoveSecretData();
        return ApiResult.Ok(result);
    }

    public async Task<ApiModels.Users.UserDetail> ProcessAsync(ulong id)
    {
        using var repository = _databaseBuilder.CreateRepository();

        var entity = await repository.User.FindUserByIdAsync(id, UserIncludeOptions.All, true)
            ?? throw new NotFoundException(_texts["User/NotFound", ApiContext.Language]);

        var result = new ApiModels.Users.UserDetail
        {
            Language = entity.Language,
            Id = entity.Id,
            Flags = entity.Flags,
            HaveBirthday = entity.Birthday is not null,
            Status = entity.Status,
            Username = entity.Username,
            SelfUnverifyMinimalTime = entity.SelfUnverifyMinimalTime,
            RegisteredAt = SnowflakeUtils.FromSnowflake(entity.Id.ToUlong()).LocalDateTime,
            AvatarUrl = entity.AvatarUrl,
            GlobalAlias = entity.GlobalAlias
        };

        var userInfo = await GetUserInfoAsync(id);

        await AddDiscordDataAsync(result);
        foreach (var guild in entity.Guilds)
            result.Guilds.Add(await CreateGuildDetailAsync(guild, userInfo));

        result.Guilds = result.Guilds.OrderByDescending(o => o.IsUserInGuild).ThenBy(o => o.Guild.Name).ToList();
        return result;
    }

    private async Task AddDiscordDataAsync(ApiModels.Users.UserDetail result)
    {
        var user = await _discordClient.FindUserAsync(result.Id.ToUlong());
        if (user is null) return;

        result.ActiveClients = user.ActiveClients.Select(o => o.ToString()).ToList();
        result.IsKnown = true;
    }

    private async Task<ApiModels.Users.GuildUserDetail> CreateGuildDetailAsync(Entity.GuildUser guildUserEntity, UserInfo? userInfo)
    {
        var result = _mapper.Map<ApiModels.Users.GuildUserDetail>(guildUserEntity);
        var guildUserInfo = userInfo?.Guilds.FirstOrDefault(o => o.GuildId == guildUserEntity.GuildId);

        result.Channels = [.. result.Channels.OrderByDescending(o => o.Count).ThenBy(o => o.Channel.Name)];
        result.Nickname = guildUserInfo?.CurrentNickname;
        result.NicknameHistory = guildUserInfo?.NicknameHistory ?? [];

        await SetCreatedInvitesAsync(result, guildUserEntity);
        await SetUsedInviteAsync(result, guildUserEntity);
        await SetUserMeasuresAsync(result, guildUserEntity);
        await SetPointsInfoAsync(result, guildUserEntity);
        await SetEmotesAsync(result, guildUserEntity);
        SetUnverify(result, guildUserInfo);

        await UpdateGuildDetailAsync(result, guildUserEntity);
        return result;
    }

    private async Task UpdateGuildDetailAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var guild = await _discordClient.GetGuildAsync(detail.Guild.Id.ToUlong());
        if (guild is null) return;

        detail.IsGuildKnown = true;

        var guildUser = await guild.GetUserAsync(entity.UserId.ToUlong());
        if (guildUser is null) return;

        detail.IsUserInGuild = true;
        await SetVisibleChannelsAsync(detail, guildUser, guild);
        detail.Roles = _mapper.Map<List<ApiModels.Role>>(guildUser.GetRoles().OrderByDescending(o => o.Position).ToList());
    }

    private async Task SetVisibleChannelsAsync(ApiModels.Users.GuildUserDetail detail, IGuildUser user, IGuild guild)
    {
        if (ApiContext.IsPublic())
            return;

        var visibleChannels = await guild.GetAvailableChannelsAsync(user);

        detail.VisibleChannels = visibleChannels
            .Where(o => o is not ICategoryChannel)
            .Select(o => _mapper.Map<ApiModels.Channels.Channel>(o))
            .OrderBy(o => o.Name)
            .ToList();
    }

    private async Task SetUserMeasuresAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var parameters = new MeasuresListParams
        {
            GuildId = entity.GuildId,
            UserId = entity.UserId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            }
        };

        var measuresResult = await _userMeasuresService.ExecuteRequestAsync((c, ctx) => c.GetMeasuresListAsync(parameters, ctx.CancellationToken));
        foreach (var measure in measuresResult.Data)
        {
            var moderator = await _dataResolveManager.GetUserAsync(measure.ModeratorId.ToUlong());

            detail.UserMeasures.Add(new ApiModels.UserMeasures.UserMeasuresItem
            {
                CreatedAt = measure.CreatedAtUtc.ToLocalTime(),
                Moderator = moderator!,
                ValidTo = measure.ValidTo?.ToLocalTime(),
                Type = measure.Type switch
                {
                    "Warning" => UserMeasuresType.Warning,
                    "Timeout" => UserMeasuresType.Timeout,
                    "Unverify" => UserMeasuresType.Unverify,
                    _ => 0
                },
                Reason = measure.Reason
            });
        }
    }

    private async Task SetPointsInfoAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        detail.HavePointsTransaction = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.ExistsAnyTransactionAsync(entity.GuildId, entity.UserId, ctx.CancellationToken));
    }

    private async Task SetEmotesAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var request = new EmoteStatisticsListRequest
        {
            GuildId = entity.GuildId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            Unsupported = false,
            UserId = entity.UserId
        };

        var response = await _emoteServiceClient.ExecuteRequestAsync((c, ctx) => c.GetEmoteStatisticsListAsync(request, ctx.CancellationToken));
        detail.Emotes = response.Data.ConvertAll(o => new ApiModels.Emotes.EmoteStatItem
        {
            Emote = o.ToEmoteItem(),
            FirstOccurence = o.FirstOccurence,
            LastOccurence = o.LastOccurence,
            UseCount = o.UseCount
        });
    }

    private async Task SetCreatedInvitesAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var request = new InviteListRequest
        {
            CreatorId = entity.UserId,
            GuildId = entity.GuildId,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            Sort =
            {
                Descending = true,
                OrderBy = "created"
            }
        };

        var invites = await _inviteServiceClient.ExecuteRequestAsync((client, ctx) => client.GetUsedInvitesAsync(request, ctx.CancellationToken));

        detail.CreatedInvites = invites.Data.ConvertAll(o => new ApiModels.Invites.InviteBase
        {
            Code = o.Code,
            CreatedAt = o.CreatedAt?.ToLocalTime()
        });
    }

    private async Task SetUsedInviteAsync(ApiModels.Users.GuildUserDetail detail, Entity.GuildUser entity)
    {
        var request = new UserInviteUseListRequest
        {
            UserId = entity.UserId,
            Pagination = {
                Page = 0,
                PageSize = int.MaxValue
            },
            Sort = { Descending = true }
        };

        var invites = await _inviteServiceClient.ExecuteRequestAsync((client, ctx) => client.GetUserInviteUsesAsync(request, ctx.CancellationToken));
        var invite = invites.Data.FirstOrDefault(o => o.GuildId == entity.GuildId);

        if (invite is null)
            return;

        var inviteInfoRequest = new InviteListRequest
        {
            GuildId = entity.GuildId,
            Code = invite.Code,
            Pagination = { PageSize = 1 }
        };

        var inviteInfo = await _inviteServiceClient.ExecuteRequestAsync((client, ctx) => client.GetUsedInvitesAsync(inviteInfoRequest, ctx.CancellationToken));
        var creator = string.IsNullOrEmpty(inviteInfo.Data[0].CreatorId) ? null : await _dataResolveManager.GetUserAsync(inviteInfo.Data[0].CreatorId.ToUlong());

        detail.UsedInvite = invite is null ? null : new ApiModels.Invites.Invite
        {
            Code = invite.Code,
            CreatedAt = inviteInfo.Data[0].CreatedAt?.ToLocalTime(),
            Creator = creator,
            UsedUsersCount = inviteInfo.Data[0].Uses,
        };
    }

    private void SetUnverify(ApiModels.Users.GuildUserDetail detail, GuildUser? guildUser)
    {
        if (guildUser?.CurrentUnverify is null)
            return;

        var endAtUtc = guildUser.CurrentUnverify.EndAtUtc.Kind == DateTimeKind.Unspecified ?
            guildUser.CurrentUnverify.EndAtUtc.WithKind(DateTimeKind.Utc) :
            guildUser.CurrentUnverify.EndAtUtc;

        var startAtUtc = guildUser.CurrentUnverify.StartAtUtc.Kind == DateTimeKind.Unspecified ?
            guildUser.CurrentUnverify.StartAtUtc.WithKind(DateTimeKind.Utc) :
            guildUser.CurrentUnverify.StartAtUtc;

        detail.Unverify = new ApiModels.Unverify.UnverifyInfo
        {
            End = endAtUtc.ToLocalTime(),
            EndTo = endAtUtc - DateTimeOffset.UtcNow,
            IsSelfUnverify = guildUser.CurrentUnverify.IsSelfUnverify,
            Reason = guildUser.CurrentUnverify.Reason,
            Start = startAtUtc.ToLocalTime()
        };
    }

    private async Task<UserInfo?> GetUserInfoAsync(ulong userId)
    {
        try
        {
            return await _userManagement.ExecuteRequestAsync((c, ctx) => c.GetUserInfoAsync(userId, ctx.CancellationToken));
        }
        catch (ClientNotFoundException)
        {
            return null;
        }
    }
}
