using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetExplicitPermissionList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IDiscordClient DiscordClient { get; }

    public GetExplicitPermissionList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        DiscordClient = discordClient;
    }

    public async Task<List<ExplicitPermission>> ProcessAsync(string query)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var permissions = await repository.Permissions.GetPermissionsListAsync(query);

        var result = new List<ExplicitPermission>();

        var userPermissions = permissions.Where(o => !o.IsRole).ToList();
        if (userPermissions.Count > 0)
        {
            var users = await repository.User.FindAllUsersExceptBots();

            var userPerms = userPermissions
                .Select(o => Mapper.Map<ExplicitPermission>(o, x => x.AfterMap((_, dst) => dst.User = Mapper.Map<Data.Models.API.Users.User>(users.Find(t => t.Id == o.TargetId)))))
                .OrderBy(o => o.User?.Username)
                .ThenBy(o => o.Command);

            result.AddRange(userPerms);
        }

        var rolePermissions = permissions.Where(o => o.IsRole).ToList();
        if (rolePermissions.Count == 0)
            return result;
        
        var roles = await DiscordClient.GetRolesAsync();
        var rolePerms = rolePermissions
            .Select(o => Mapper.Map<ExplicitPermission>(o, x => x.AfterMap((_, dst) => dst.Role = Mapper.Map<Role>(roles.Find(t => t.Id.ToString() == o.TargetId)))))
            .OrderBy(o => o.Role?.Name)
            .ThenBy(o => o.Command);

        result.AddRange(rolePerms);
        return result;
    }
}
