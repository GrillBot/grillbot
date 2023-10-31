using GrillBot.Common.Models;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Services.RubbergodService.Models.Karma;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V2.User;

public class GetRubbergodUserKarma : ApiAction
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetRubbergodUserKarma(ApiRequestContext apiContext, IRubbergodServiceClient rubbergodServiceClient, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        RubbergodServiceClient = rubbergodServiceClient;
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (KarmaListParams)Parameters[0]!;
        var page = await RubbergodServiceClient.GetKarmaPageAsync(parameters.Pagination);
        var users = await ReadUsersAsync(page.Data.ConvertAll(o => o.UserId));
        var result = await PaginatedResponse<KarmaListItem>.CopyAndMapAsync(page, entity => Task.FromResult(Map(entity, users)));

        return ApiResult.Ok(result);
    }

    private async Task<Dictionary<string, Database.Entity.User>> ReadUsersAsync(List<string> userIds)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var users = await repository.User.GetUsersByIdsAsync(userIds);

        return users.ToDictionary(o => o.Id, o => o);
    }

    private static KarmaListItem Map(UserKarma entity, IReadOnlyDictionary<string, Database.Entity.User> users)
    {
        return new KarmaListItem
        {
            User = FindAndMapUser(entity.UserId, users),
            Negative = entity.Negative,
            Position = entity.Position,
            Positive = entity.Positive,
            Value = entity.Value
        };
    }

    private static Data.Models.API.Users.User FindAndMapUser(string userId, IReadOnlyDictionary<string, Database.Entity.User> users)
    {
        if (users.TryGetValue(userId, out var userEntity))
        {
            return new Data.Models.API.Users.User
            {
                Username = userEntity.Username,
                AvatarUrl = userEntity.AvatarUrl ?? CDN.GetDefaultUserAvatarUrl(0UL),
                IsBot = userEntity.HaveFlags(UserFlags.NotUser),
                Id = userEntity.Id
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
