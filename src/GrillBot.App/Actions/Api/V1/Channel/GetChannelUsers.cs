﻿using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Channels;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetChannelUsers : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetChannelUsers(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<PaginatedResponse<ChannelUserStatItem>> ProcessAsync(ulong channelId, PaginatedParams pagination)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Channel.GetUserChannelListAsync(channelId, pagination);
        var result = await PaginatedResponse<ChannelUserStatItem>
            .CopyAndMapAsync(data, entity => Task.FromResult(Mapper.Map<ChannelUserStatItem>(entity)));

        for (var i = 0; i < result.Data.Count; i++)
            result.Data[i].Position = pagination.Skip() + i + 1;
        return result;
    }
}
