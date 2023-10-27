﻿using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Data.Models.API.Points;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetTransactionList : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public GetTransactionList(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, IPointsServiceClient pointsServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        PointsServiceClient = pointsServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (AdminListRequest)Parameters[0]!;

        var transactions = await PointsServiceClient.GetTransactionListAsync(request);
        transactions.ValidationErrors.AggregateAndThrow();

        var guildCache = new Dictionary<string, Data.Models.API.Guilds.Guild>();
        var userCache = new Dictionary<string, Data.Models.API.Users.User>();

        await using var repository = DatabaseBuilder.CreateRepository();

        var result = await PaginatedResponse<PointsTransaction>.CopyAndMapAsync(transactions.Response!, async entity =>
        {
            if (!guildCache.TryGetValue(entity.GuildId, out var guild))
            {
                var dbGuild = await repository.Guild.FindGuildByIdAsync(entity.GuildId.ToUlong(), true);
                guild = Mapper.Map<Data.Models.API.Guilds.Guild>(dbGuild);
                guildCache.Add(entity.GuildId, guild);
            }

            if (!userCache.TryGetValue(entity.UserId, out var user))
            {
                var dbUser = await repository.User.FindUserByIdAsync(entity.UserId.ToUlong(), UserIncludeOptions.None, true);
                user = Mapper.Map<Data.Models.API.Users.User>(dbUser);
                userCache.Add(entity.UserId, user);
            }

            var mergeInfo = entity.MergedCount > 0
                ? new PointsMergeInfo
                {
                    MergedItemsCount = entity.MergedCount,
                    MergeRangeFrom = entity.MergedFrom.GetValueOrDefault(),
                    MergeRangeTo = entity.MergedTo.GetValueOrDefault()
                }
                : null;

            if (mergeInfo != null && mergeInfo.MergeRangeFrom == mergeInfo.MergeRangeTo)
                mergeInfo.MergeRangeTo = null;

            return new PointsTransaction
            {
                Points = entity.Value,
                CreatedAt = entity.CreatedAt.ToLocalTime(),
                ReactionId = entity.ReactionId,
                MessageId = entity.MessageId,
                MergeInfo = mergeInfo,
                Guild = guild,
                User = user
            };
        });

        return ApiResult.Ok(result);
    }
}
