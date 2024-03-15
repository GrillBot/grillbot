using AutoMapper;
using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetCurrentUnverifies : ApiAction
{
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public GetCurrentUnverifies(ApiRequestContext apiContext, IMapper mapper, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var data = await GetAllUnverifiesAsync(
            ApiContext.IsPublic() ? ApiContext.GetUserId() : null
        );

        var result = new List<UnverifyUserProfile>();
        foreach (var entity in data)
        {
            var profile = Mapper.Map<UnverifyUserProfile>(entity.profile);
            profile.Guild = Mapper.Map<Data.Models.API.Guilds.Guild>(entity.guild);

            result.Add(profile);
        }

        return ApiResult.Ok(result);
    }

    private async Task<List<(Data.Models.Unverify.UnverifyUserProfile profile, IGuild guild)>> GetAllUnverifiesAsync(ulong? userId = null)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var unverifies = await repository.Unverify.GetUnverifiesAsync(userId);

        var profiles = new List<(Data.Models.Unverify.UnverifyUserProfile profile, IGuild guild)>();
        foreach (var unverify in unverifies)
        {
            var guild = await DiscordClient.GetGuildAsync(unverify.GuildId.ToUlong());
            if (guild is null) continue;

            var user = await guild.GetUserAsync(unverify.UserId.ToUlong());
            var profile = UnverifyProfileManager.Reconstruct(unverify, user, guild);

            profiles.Add((profile, guild));
        }

        return profiles;
    }
}
