using System.Diagnostics.CodeAnalysis;
using System.IO;
using Discord;
using GrillBot.App.Actions.Commands.Points;
using GrillBot.Cache.Services.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Points;

[TestClass]
public class PointsImageTests : CommandActionTest<PointsImage>
{
    private static readonly IUser MockUser = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetAvatar(Consts.AvatarId).Build();
    private static readonly IGuildUser GuildUser = new GuildUserBuilder(MockUser).SetAvatar(Consts.AvatarId).Build();
    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetUserAction(GuildUser).Build();

    protected override IGuild Guild => GuildData;

    protected override IGuildUser User { get; }
        = new GuildUserBuilder(GuildUser).SetAvatar(Consts.AvatarId).SetGuild(GuildData).Build();

    protected override PointsImage CreateAction()
    {
        var profilePictureManager = new ProfilePictureManager(CacheBuilder, TestServices.CounterManager.Value);
        return InitAction(new PointsImage(DatabaseBuilder, profilePictureManager));
    }

    private async Task InitDataAsync()
    {
        await Repository.Guild.GetOrCreateGuildAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(User);

        await Repository.AddAsync(new PointsTransaction
        {
            Points = 10,
            AssingnedAt = DateTime.MinValue,
            GuildId = Consts.GuildId.ToString(),
            MessageId = Consts.MessageId.ToString(),
            ReactionId = Consts.PepeJamEmote,
            UserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessAsync_UserNotFound()
        => await Action.ProcessAsync(Guild, User);

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();

        using var result = await Action.ProcessAsync(Guild, User);
        Assert.IsTrue(File.Exists(result.Path));
    }
}
