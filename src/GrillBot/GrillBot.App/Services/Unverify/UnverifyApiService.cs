using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Org.BouncyCastle.Crypto;
using System.Security.Claims;

namespace GrillBot.App.Services.Unverify;

public class UnverifyApiService : ServiceBase
{
    public UnverifyApiService(GrillBotContextFactory dbFactory, IMapper mapper,
        IDiscordClient discordClient) : base(null, dbFactory, null, discordClient, mapper)
    {
    }

    public async Task<PaginatedResponse<UnverifyLogItem>> GetLogsAsync(UnverifyLogParams @params, ClaimsPrincipal loggedUser,
        CancellationToken cancellationToken)
    {
        using var context = DbFactory.Create();

        var query = context.UnverifyLogs.AsNoTracking()
            .Include(o => o.FromUser.User)
            .Include(o => o.Guild)
            .Include(o => o.ToUser.User)
            .AsSplitQuery();

        if (loggedUser.HaveUserPermission())
        {
            var loggedUserId = loggedUser.GetUserId();
            @params.FromUserId = null;
            @params.ToUserId = loggedUserId.ToString();

            var mutualGuilds = (await DcClient.FindMutualGuildsAsync(loggedUserId))
                .ConvertAll(o => o.Id.ToString());

            query = @params.CreateQuery(query)
                .Where(o => mutualGuilds.Contains(o.GuildId));
        }
        else
        {
            query = @params.CreateQuery(query);
        }

        return await PaginatedResponse<UnverifyLogItem>
            .CreateAsync(query, @params, async (entity, cancellationToken) => await MapItemAsync(entity, cancellationToken), cancellationToken);
    }

    private async Task<UnverifyLogItem> MapItemAsync(UnverifyLog entity, CancellationToken cancellationToken)
    {
        var guild = await DcClient.GetGuildAsync(Convert.ToUInt64(entity.GuildId), options: new() { CancelToken = cancellationToken });
        var result = Mapper.Map<UnverifyLogItem>(entity);

        switch (entity.Operation)
        {
            case UnverifyOperation.Autoremove:
            case UnverifyOperation.Recover:
            case UnverifyOperation.Remove:
                {
                    var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogRemove>(entity.Data);
                    result.RemoveData = new UnverifyLogRemove()
                    {
                        ReturnedChannelIds = jsonData.ReturnedOverwrites.ConvertAll(o => o.ChannelId.ToString()),
                        ReturnedRoles = jsonData.ReturnedRoles.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o)))
                    };
                }
                break;
            case UnverifyOperation.Selfunverify:
            case UnverifyOperation.Unverify:
                {
                    var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogSet>(entity.Data);
                    result.SetData = new UnverifyLogSet()
                    {
                        ChannelIdsToKeep = jsonData.ChannelsToKeep.ConvertAll(o => o.ChannelId.ToString()),
                        ChannelIdsToRemove = jsonData.ChannelsToRemove.ConvertAll(o => o.ChannelId.ToString()),
                        End = jsonData.End,
                        IsSelfUnverify = jsonData.IsSelfUnverify,
                        Reason = jsonData.Reason,
                        RolesToKeep = jsonData.RolesToKeep.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o))),
                        RolesToRemove = jsonData.RolesToRemove.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o))),
                        Start = jsonData.Start
                    };
                }
                break;
            case UnverifyOperation.Update:
                {
                    var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogUpdate>(entity.Data);
                    result.UpdateData = new UnverifyLogUpdate()
                    {
                        End = jsonData.End,
                        Start = jsonData.Start
                    };
                }
                break;
        }

        return result;
    }
}
