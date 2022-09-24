﻿using GrillBot.App.Actions.Api.V1.Invite;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Invite;

[TestClass]
public class GetInviteListTests : ApiActionTest<GetInviteList>
{
    protected override GetInviteList CreateAction()
    {
        return new GetInviteList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));

        var guildUser = Database.Entity.GuildUser.FromDiscord(guild, user);
        guildUser.UsedInvite = new Database.Entity.Invite { Code = Consts.InviteCode, CreatorId = Consts.UserId.ToString(), GuildId = Consts.GuildId.ToString() };
        await Repository.AddAsync(guildUser);
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(new GetInviteListParams());
        Assert.AreEqual(1, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetInviteListParams
        {
            Code = Consts.InviteCode,
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            CreatorId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString()
        };

        var result = await Action.ProcessAsync(filter);
        Assert.AreEqual(0, result.TotalItemsCount);
    }
}
