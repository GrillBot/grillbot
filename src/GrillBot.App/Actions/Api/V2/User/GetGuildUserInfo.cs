using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using UnverifyService;

namespace GrillBot.App.Actions.Api.V2.User;

public class GetGuildUserInfo(
    ApiRequestContext apiContext,
    GrillBotDatabaseBuilder _databaseBuilder,
    IServiceClientExecutor<IUserMeasuresServiceClient> _userMeasuresService,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    DataResolveManager _dataResolve
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = GetParameter<string>(0);
        var userId = GetParameter<string>(1);

        using var repository = _databaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserByIdAsync(guildId.ToUlong(), userId.ToUlong(), true);
        if (guildUser is null)
            return ApiResult.NotFound();

        var guild = (await _dataResolve.GetGuildAsync(guildId.ToUlong(), CancellationToken)) ?? new();
        var result = new GuildUserInfo
        {
            AvatarUrl = guildUser.User!.AvatarUrl,
            GlobalAlias = guildUser.User.GlobalAlias,
            Guild = guild,
            Id = guildUser.UserId,
            IsBot = guildUser.User.HaveFlags(UserFlags.NotUser),
            Username = guildUser.User.Username,
            SelfUnverifyCount = await ComputeSelfUnverifyCountAsync(guildId, userId)
        };

        var userMeasuresInfo = await _userMeasuresService.ExecuteRequestAsync(
            (c, ctx) => c.GetUserInfoAsync(guildId, userId, ctx.CancellationToken),
            CancellationToken
        );

        result.WarningCount = userMeasuresInfo.WarningCount;
        result.UnverifyCount = userMeasuresInfo.UnverifyCount;

        return ApiResult.Ok(result);
    }

    private async Task<int> ComputeSelfUnverifyCountAsync(string guildId, string userId)
    {
        try
        {
            var userInfo = await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.GetUserInfoAsync(userId.ToUlong(), ctx.CancellationToken),
                CancellationToken
            );

            return userInfo?.SelfUnverifyCount.TryGetValue(guildId, out var count) == true ? count : 0;
        }
        catch (ClientNotFoundException)
        {
            return 0;
        }
    }
}
