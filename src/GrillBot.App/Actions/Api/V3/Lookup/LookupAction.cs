using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Enums;

namespace GrillBot.App.Actions.Api.V3.DataResolve;

public class LookupAction : ApiAction
{
    private readonly DataResolveManager _dataResolve;

    public LookupAction(ApiRequestContext apiContext, DataResolveManager dataResolve) : base(apiContext)
    {
        _dataResolve = dataResolve;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var type = GetParameter<DataResolveType>(0);

        return type switch
        {
            DataResolveType.Guild => ResolveGuildAsync(),
            DataResolveType.Channel => ResolveChannelAsync(),
            DataResolveType.Role => ResolveRoleAsync(),
            DataResolveType.User => ResolveUserAsync(),
            DataResolveType.GuildUser => ResolveGuildUserAsync(),
            _ => throw new NotSupportedException()
        };
    }

    private async Task<ApiResult> ResolveGuildAsync()
    {
        var guildId = GetParameter<ulong>(1);
        return CreateResult(await _dataResolve.GetGuildAsync(guildId));
    }

    private async Task<ApiResult> ResolveChannelAsync()
    {
        var guildId = GetParameter<ulong>(1);
        var channelId = GetParameter<ulong>(2);

        return CreateResult(await _dataResolve.GetChannelAsync(guildId, channelId));
    }

    private async Task<ApiResult> ResolveRoleAsync()
    {
        var roleId = GetParameter<ulong>(1);
        return CreateResult(await _dataResolve.GetRoleAsync(roleId));
    }

    private async Task<ApiResult> ResolveUserAsync()
    {
        var userId = GetParameter<ulong>(1);
        return CreateResult(await _dataResolve.GetUserAsync(userId));
    }

    private async Task<ApiResult> ResolveGuildUserAsync()
    {
        var guildId = GetParameter<ulong>(1);
        var userId = GetParameter<ulong>(2);
        return CreateResult(await _dataResolve.GetGuildUserAsync(guildId, userId));
    }

    private static ApiResult CreateResult(object? result)
        => result is null ? ApiResult.NotFound() : ApiResult.Ok(result);
}
