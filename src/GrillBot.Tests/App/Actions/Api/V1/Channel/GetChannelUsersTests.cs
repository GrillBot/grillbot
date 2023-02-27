using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Common.Models.Pagination;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class GetChannelUsersTests : ApiActionTest<GetChannelUsers>
{
    protected override GetChannelUsers CreateInstance()
    {
        return new GetChannelUsers(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(user).SetGuild(guild).Build();
        var textChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(textChannel, ChannelType.Text));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, guildUser));
        await Repository.AddAsync(new Database.Entity.GuildUserChannel
        {
            ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString(),
            Count = 50, FirstMessageAt = DateTime.Now, LastMessageAt = DateTime.Now
        });
        await Repository.CommitAsync();

        var pagination = new PaginatedParams();
        var result = await Instance.ProcessAsync(Consts.ChannelId, pagination);

        Assert.AreEqual(1, result.Data.Count);
        Assert.AreEqual(1, result.TotalItemsCount);
        Assert.IsFalse(result.CanNext);
        Assert.IsFalse(result.CanPrev);
        Assert.AreEqual(0, result.Page);
    }
}
