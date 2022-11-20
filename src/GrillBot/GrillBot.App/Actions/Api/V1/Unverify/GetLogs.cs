using AutoMapper;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetLogs : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetLogs(ApiRequestContext apiContext, IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DiscordClient = discordClient;
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<PaginatedResponse<UnverifyLogItem>> ProcessAsync(UnverifyLogParams parameters)
    {
        var mutualGuilds = await GetMutualGuildsAsync();
        UpdatePublicAccess(parameters, mutualGuilds);

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Unverify.GetLogsAsync(parameters, parameters.Pagination, mutualGuilds);
        return await PaginatedResponse<UnverifyLogItem>.CopyAndMapAsync(data, MapItemAsync);
    }

    private async Task<List<string>> GetMutualGuildsAsync()
    {
        if (!ApiContext.IsPublic()) return new List<string>();

        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        return mutualGuilds.ConvertAll(o => o.Id.ToString());
    }

    private void UpdatePublicAccess(UnverifyLogParams parameters, ICollection<string> mutualGuilds)
    {
        if (!ApiContext.IsPublic()) return;

        parameters.FromUserId = null;
        parameters.ToUserId = ApiContext.GetUserId().ToString();
        if (!string.IsNullOrEmpty(parameters.GuildId) && !mutualGuilds.Contains(parameters.GuildId))
            parameters.GuildId = null;
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
                var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogRemove>(entity.Data)!;
                result.RemoveData = new UnverifyLogRemove
                {
                    ReturnedChannelIds = jsonData.ReturnedOverwrites.ConvertAll(o => o.ChannelId.ToString()),
                    ReturnedRoles = jsonData.ReturnedRoles.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o))),
                    FromWeb = jsonData.FromWeb
                };
            }
                break;
            case UnverifyOperation.Selfunverify:
            case UnverifyOperation.Unverify:
            {
                var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogSet>(entity.Data)!;
                result.SetData = new UnverifyLogSet
                {
                    ChannelIdsToKeep = jsonData.ChannelsToKeep.ConvertAll(o => o.ChannelId.ToString()),
                    ChannelIdsToRemove = jsonData.ChannelsToRemove.ConvertAll(o => o.ChannelId.ToString()),
                    End = jsonData.End,
                    IsSelfUnverify = jsonData.IsSelfUnverify,
                    Reason = jsonData.Reason,
                    RolesToKeep = jsonData.RolesToKeep.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o))),
                    RolesToRemove = jsonData.RolesToRemove.ConvertAll(o => Mapper.Map<Role>(guild.GetRole(o))),
                    Start = jsonData.Start,
                    Language = jsonData.Language
                };
            }
                break;
            case UnverifyOperation.Update:
            {
                var jsonData = JsonConvert.DeserializeObject<Data.Models.Unverify.UnverifyLogUpdate>(entity.Data)!;
                result.UpdateData = new UnverifyLogUpdate
                {
                    End = jsonData.End,
                    Start = jsonData.Start,
                    Reason = jsonData.Reason
                };
            }
                break;
            default:
                throw new ArgumentOutOfRangeException($"Invalid operation type ({entity.Operation})", nameof(entity.Operation));
        }

        return result;
    }
}
