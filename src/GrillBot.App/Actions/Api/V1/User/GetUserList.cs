using AutoMapper;
using GrillBot.Common.Extensions;
using GrillBot.Common.Models;
using GrillBot.Common.Models.Pagination;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Actions.Api.V1.User;

public class GetUserList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }

    public GetUserList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
    }

    public async Task<PaginatedResponse<UserListItem>> ProcessAsync(GetUserListParams parameters)
    {
        parameters.FixStatus();

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.User.GetUsersListAsync(parameters, parameters.Pagination);
        return await PaginatedResponse<UserListItem>.CopyAndMapAsync(data, MapItemAsync);
    }

    private async Task<UserListItem> MapItemAsync(Database.Entity.User entity)
    {
        var result = Mapper.Map<UserListItem>(entity);

        foreach (var guild in entity.Guilds.OrderBy(o => o.Guild!.Name))
        {
            var discordGuild = await DiscordClient.GetGuildAsync(guild.GuildId.ToUlong());
            var guildUser = discordGuild != null ? await discordGuild.GetUserAsync(guild.UserId.ToUlong()) : null;

            result.Guilds.Add(guild.Guild!.Name, guildUser != null);
        }

        return result;
    }
}
