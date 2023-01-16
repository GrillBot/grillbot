﻿using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Helpers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class RolesReaderTests : CommandActionTest<RolesReader>
{
    private static readonly IRole[] Roles =
    {
        new RoleBuilder(Consts.RoleId, Consts.RoleName).SetColor(Color.Blue).SetPermissions(GuildPermissions.All).SetPosition(1).SetTags(RoleHelper.CreateTags(Consts.UserId, null, false)).Build(),
        new RoleBuilder(Consts.RoleId + 1, Consts.RoleName).SetColor(Color.Red).SetPermissions(GuildPermissions.All).SetPosition(2).SetTags(RoleHelper.CreateTags(null, null, true)).Build(),
        new RoleBuilder(Consts.RoleId + 2, Consts.RoleName).SetColor(Color.Green).SetPermissions(GuildPermissions.Webhook).SetPosition(2).SetTags(RoleHelper.CreateTags(null, null, true)).Build()
    };

    private static readonly IGuild EmptyGuild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetRoles(Roles).Build();

    private static readonly IGuildUser[] GuildUsers =
    {
        new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).AsBot().SetGuild(EmptyGuild).SetRoles(new[] { Roles[0].Id }).Build(),
        new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).AsBot().SetGuild(EmptyGuild).SetRoles(new[] { Roles[1].Id }).Build()
    };

    protected override IGuild Guild => new GuildBuilder(EmptyGuild.Id, EmptyGuild.Name).SetRoles(Roles).SetGetUsersAction(GuildUsers).Build();
    protected override IGuildUser User => GuildUsers[1];

    protected override RolesReader CreateAction()
    {
        var texts = TestServices.Texts.Value;
        var formatHelper = new FormatHelper(texts);
        return InitAction(new RolesReader(formatHelper, texts));
    }

    [TestMethod]
    public async Task ProcessListAsync()
    {
        var result = await Action.ProcessListAsync("position");

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Fields.Length);
    }

    [TestMethod]
    public async Task ProcessListAsync_SortByMembers()
    {
        var result = await Action.ProcessListAsync("members");

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Fields.Length);
    }

    [TestMethod]
    public async Task ProcessDetailAsync()
    {
        foreach (var role in Roles)
        {
            var result = await Action.ProcessDetailAsync(role);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Fields.Length >= 5);
        }
    }
}
