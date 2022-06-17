using AutoMapper;
using GrillBot.Common.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Common.Models;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Unverify;

public class UnverifyApiService
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public UnverifyApiService(GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient,
        ApiRequestContext apiRequestContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
        ApiRequestContext = apiRequestContext;
    }

    public async Task<PaginatedResponse<UnverifyLogItem>> GetLogsAsync(UnverifyLogParams parameters)
    {
        if (ApiRequestContext.IsPublic())
        {
            var loggedUserId = ApiRequestContext.LoggedUserId;
            var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);

            parameters.FromUserId = null;
            parameters.ToUserId = loggedUserId.ToString();
            parameters.MutualGuilds = mutualGuilds.ConvertAll(o => o.Id.ToString());
        }

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Unverify.GetLogsAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<UnverifyLogItem>.CopyAndMapAsync(data, MapItemAsync);
    }

    private async Task<UnverifyLogItem> MapItemAsync(UnverifyLog entity)
    {
        var guild = await DiscordClient.GetGuildAsync(entity.GuildId.ToUlong());
        var result = Mapper.Map<UnverifyLogItem>(entity);

        switch (entity.Operation)
        {
            case UnverifyOperation.Autoremove:
            case UnverifyOperation.Recover:
            case UnverifyOperation.Remove:
            {
                var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogRemove>(entity.Data);
                if (jsonData != null)
                {
                    result.RemoveData = new UnverifyLogRemove
                    {
                        ReturnedChannelIds = jsonData.ReturnedOverwrites.ConvertAll(o => o.ChannelId.ToString()),
                        ReturnedRoles = jsonData.ReturnedRoles.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o)))
                    };
                }
            }
                break;
            case UnverifyOperation.Selfunverify:
            case UnverifyOperation.Unverify:
            {
                var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogSet>(entity.Data);
                if (jsonData != null)
                {
                    result.SetData = new UnverifyLogSet
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
            }
                break;
            case UnverifyOperation.Update:
            {
                var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogUpdate>(entity.Data);
                if (jsonData != null)
                {
                    result.UpdateData = new UnverifyLogUpdate
                    {
                        End = jsonData.End,
                        Start = jsonData.Start
                    };
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException($"Invalid operation type ({entity.Operation})", nameof(entity.Operation));
        }

        return result;
    }
}
