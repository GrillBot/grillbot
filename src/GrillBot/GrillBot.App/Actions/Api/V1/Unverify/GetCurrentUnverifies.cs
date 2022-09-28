using AutoMapper;
using GrillBot.App.Services.Unverify;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class GetCurrentUnverifies : ApiAction
{
    private UnverifyService UnverifyService { get; }
    private IMapper Mapper { get; }

    public GetCurrentUnverifies(ApiRequestContext apiContext, UnverifyService unverifyService, IMapper mapper) : base(apiContext)
    {
        UnverifyService = unverifyService;
        Mapper = mapper;
    }

    public async Task<List<UnverifyUserProfile>> ProcessAsync()
    {
        var data = await UnverifyService.GetAllUnverifiesAsync(
            ApiContext.IsPublic() ? ApiContext.GetUserId() : null
        );

        var result = new List<UnverifyUserProfile>();
        foreach (var entity in data)
        {
            var profile = Mapper.Map<UnverifyUserProfile>(entity.profile);
            profile.Guild = Mapper.Map<Data.Models.API.Guilds.Guild>(entity.guild);

            result.Add(profile);
        }

        return result;
    }
}
