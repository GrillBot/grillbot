using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System.Security.Claims;

namespace GrillBot.App.Services.Unverify;

public class UnverifyApiService : ServiceBase
{
    public UnverifyApiService(GrillBotDatabaseBuilder dbFactory, IMapper mapper,
        IDiscordClient discordClient) : base(null, dbFactory, discordClient, mapper)
    {
    }

    public async Task<PaginatedResponse<UnverifyLogItem>> GetLogsAsync(UnverifyLogParams parameters, ClaimsPrincipal loggedUser,
        CancellationToken cancellationToken)
    {
        if (loggedUser.HaveUserPermission())
        {
            var loggedUserId = loggedUser.GetUserId();

            parameters.FromUserId = null;
            parameters.ToUserId = loggedUserId.ToString();
            parameters.MutualGuilds = (await DcClient.FindMutualGuildsAsync(loggedUserId)).ConvertAll(o => o.Id.ToString());
        }

        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters, true);

        return await PaginatedResponse<UnverifyLogItem>
            .CreateAsync(query, parameters.Pagination, (entity, cancellationToken) => MapItemAsync(entity, cancellationToken), cancellationToken);
    }

    private async Task<UnverifyLogItem> MapItemAsync(UnverifyLog entity, CancellationToken cancellationToken)
    {
        var guild = await DcClient.GetGuildAsync(entity.GuildId.ToUlong(), options: new() { CancelToken = cancellationToken });
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
