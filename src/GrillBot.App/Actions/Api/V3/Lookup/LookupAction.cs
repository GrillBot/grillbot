using GrillBot.App.Helpers;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.API;

namespace GrillBot.App.Actions.Api.V3.Lookup;

public class LookupAction : ApiAction
{
    private readonly DataResolveManager _dataResolve;
    private readonly BlobManagerFactoryHelper _blobManagerHelper;

    public LookupAction(ApiRequestContext apiContext, DataResolveManager dataResolve, BlobManagerFactoryHelper blobManagerHelper) : base(apiContext)
    {
        _dataResolve = dataResolve;
        _blobManagerHelper = blobManagerHelper;
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
            DataResolveType.FileSasLink => ResolveSasLinkAsync(),
            _ => Task.FromException<ApiResult>(new NotSupportedException())
        };
    }

    private async Task<ApiResult> ResolveGuildAsync()
    {
        var guildId = GetParameter<ulong>(1);
        return CreateResult(await _dataResolve.GetGuildAsync(guildId));
    }

    private async Task<ApiResult> ResolveChannelAsync()
    {
        var channelId = GetParameter<ulong>(1);
        return CreateResult(await _dataResolve.GetChannelAsync(channelId));
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

    private async Task<ApiResult> ResolveSasLinkAsync()
    {
        var filename = GetParameter<string>(1);

        var containers = typeof(BlobConstants)
            .GetConstants()
            .Select(o => o.GetValue(null)?.ToString())
            .Where(o => !string.IsNullOrEmpty(o));

        foreach (var container in containers)
        {
            var manager = await _blobManagerHelper.CreateAsync(container!);

            if (await manager.ExistsAsync(filename))
            {
                var link = manager.GenerateSasLink(filename, 1);
                return CreateResult(string.IsNullOrEmpty(link) ? null : new MessageResponse(link));
            }
        }

        return ApiResult.NotFound();
    }


    private static ApiResult CreateResult(object? result)
        => result is null ? ApiResult.NotFound() : ApiResult.Ok(result);
}
