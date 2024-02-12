﻿using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V2.User;

public class GetGuildUserInfo : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IUserMeasuresServiceClient UserMeasuresService { get; }
    private IMapper Mapper { get; }

    public GetGuildUserInfo(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IUserMeasuresServiceClient userMeasuresService,
        IMapper mapper) : base(apiContext)
    {
        UserMeasuresService = userMeasuresService;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (string)Parameters[0]!;
        var userId = (string)Parameters[1]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserByIdAsync(guildId.ToUlong(), userId.ToUlong(), true);
        if (guildUser is null)
            return ApiResult.NotFound();

        var result = new GuildUserInfo
        {
            AvatarUrl = guildUser.User!.AvatarUrl,
            GlobalAlias = guildUser.User.GlobalAlias,
            Guild = Mapper.Map<Data.Models.API.Guilds.Guild>(guildUser.Guild),
            Id = guildUser.UserId,
            IsBot = guildUser.User.HaveFlags(UserFlags.NotUser),
            Username = guildUser.User.Username,
            SelfUnverifyCount = await ComputeSelfUnverifyCountAsync(repository, guildId, userId)
        };

        var userMeasuresInfo = await UserMeasuresService.GetUserInfoAsync(guildId, userId);
        result.WarningCount = userMeasuresInfo.WarningCount;
        result.UnverifyCount = userMeasuresInfo.UnverifyCount;

        return ApiResult.Ok(result);
    }

    private static async Task<int> ComputeSelfUnverifyCountAsync(GrillBotRepository repository, string guildId, string userId)
    {
        var (_, selfunverify) = await repository.Unverify.GetUserStatsAsync(guildId, userId);
        return selfunverify;
    }
}
