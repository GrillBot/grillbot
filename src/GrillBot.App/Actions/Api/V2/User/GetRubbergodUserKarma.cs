using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Common.Services.RubbergodService.Models.Karma;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Actions.Api.V2.User;

public class GetRubbergodUserKarma : ApiAction
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private IDiscordClient DiscordClient { get; }

    public GetRubbergodUserKarma(ApiRequestContext apiContext, IRubbergodServiceClient rubbergodServiceClient, IDiscordClient discordClient) : base(apiContext)
    {
        RubbergodServiceClient = rubbergodServiceClient;
        DiscordClient = discordClient;
    }

    public async Task<PaginatedResponse<KarmaListItem>> ProcessAsync(KarmaListParams parameters)
    {
        var page = await RubbergodServiceClient.GetKarmaPageAsync(parameters.Pagination);
        return await PaginatedResponse<KarmaListItem>.CopyAndMapAsync(page, MapAsync);
    }

    private async Task<KarmaListItem> MapAsync(UserKarma entity)
    {
        return new KarmaListItem
        {
            User = await FindAndMapUserAsync(entity.UserId),
            Negative = entity.Negative,
            Position = entity.Position,
            Positive = entity.Positive,
            Value = entity.Value
        };
    }

    private async Task<Data.Models.API.Users.User> FindAndMapUserAsync(string userId)
    {
        var user = await DiscordClient.FindUserAsync(userId.ToUlong());
        if (user is not null)
        {
            return new Data.Models.API.Users.User
            {
                Id = user.Id.ToString(),
                Username = user.GlobalName ?? user.Username, // TODO Review after usernames rework.
                AvatarUrl = user.GetUserAvatarUrl(),
                IsBot = user.IsUser()
            };
        }

        return new Data.Models.API.Users.User
        {
            Username = "Deleted user",
            AvatarUrl = CDN.GetDefaultUserAvatarUrl(0UL),
            IsBot = false,
            Id = userId
        };
    }
}
