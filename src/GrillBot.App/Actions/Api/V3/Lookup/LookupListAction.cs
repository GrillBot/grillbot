using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.App.Actions.Api.V3.Lookup;

public class LookupListAction(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    GrillBotContext _dbContext
) : ApiAction(apiContext)
{
    public override Task<ApiResult> ProcessAsync()
    {
        var type = GetParameter<DataResolveType>(0);

        return type switch
        {
            DataResolveType.Guild => ResolveGuildListAsync(),
            DataResolveType.User => ResolveUserListAsync(),
            DataResolveType.Channel => ResolveChannelListAsync(),
            _ => Task.FromException<ApiResult>(new NotSupportedException())
        };
    }

    private async Task<ApiResult> ResolveGuildListAsync()
    {
        if (ApiContext.IsPublic())
        {
            var mutualGuilds = await GetMutualGuildsAsync();

            return ApiResult.Ok(mutualGuilds.ConvertAll(o => new Guild
            {
                Id = o.Id.ToString(),
                IsConnected = true,
                MemberCount = o.GetMemberCount(),
                Name = o.Name,
            }));
        }

        var query = _dbContext.Guilds.AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new Guild
            {
                Name = o.Name,
                Id = o.Id,
                MemberCount = o.Users.Count()
            });

        var guilds = await query.ToListAsync();
        return ApiResult.Ok(guilds);
    }

    private async Task<ApiResult> ResolveUserListAsync()
    {
        var query = _dbContext.Users
            .OrderBy(o => o.GlobalAlias ?? o.Username)
            .ThenBy(o => o.Username)
            .ThenBy(o => o.Id)
            .AsNoTracking();

        if (ApiContext.IsPublic())
        {
            var mutualGuilds = (await GetMutualGuildsAsync()).Select(o => o.Id.ToString()).ToArray();
            query = query.Where(o => o.Guilds.Any(g => mutualGuilds.Contains(g.GuildId)));
        }

        var mappedQuery = query.Select(o => new User
        {
            Id = o.Id,
            AvatarUrl = o.AvatarUrl ?? "",
            GlobalAlias = o.GlobalAlias,
            IsBot = (o.Flags & (int)UserFlags.NotUser) != 0,
            Username = o.Username
        });

        var users = await mappedQuery.ToListAsync();
        return ApiResult.Ok(users);
    }

    private Task<List<IGuild>> GetMutualGuildsAsync()
    {
        var loggedUserId = ApiContext.GetUserId();
        return _discordClient.FindMutualGuildsAsync(loggedUserId);
    }

    private async Task<ApiResult> ResolveChannelListAsync()
    {
        if (ApiContext.IsPublic())
        {
            var guilds = await GetMutualGuildsAsync();
            var visibleChannels = new List<Channel>();
            var userId = ApiContext.GetUserId();

            foreach (var guild in guilds)
            {
                var user = await guild.GetUserAsync(userId);
                visibleChannels.AddRange(
                    (await guild.GetAvailableChannelsAsync(user))
                        .Select(o => new Channel
                        {
                            Id = o.Id.ToString(),
                            Name = o.HaveCategory() ? $"{o.Name} ({o.GetCategory().GetPropertyValue(x => x.Name)})".Replace("()", "").TrimEnd() : o.Name,
                            Type = o.GetChannelType()
                        })
                        .Where(ch => ch.Type != ChannelType.Category && ch.Type != ChannelType.DM)
                );
            }

            return ApiResult.Ok(visibleChannels);
        }

        var baseQuery = _dbContext.Channels
            .Where(ch => ch.ChannelType != ChannelType.Category && ch.ChannelType != ChannelType.DM)
            .OrderBy(o => o.Name)
            .AsNoTracking();

        var query = baseQuery.Select(o => new Channel
        {
            Name = o.Name,
            Flags = o.Flags,
            Id = o.ChannelId,
            Type = o.ChannelType
        });

        var channels = await query.ToListAsync();
        return ApiResult.Ok(channels);
    }
}
