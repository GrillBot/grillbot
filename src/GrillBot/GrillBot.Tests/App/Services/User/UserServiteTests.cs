using Discord;
using GrillBot.App.Services.User;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User;

[TestClass]
public class UserServiteTests : ServiceTest<UserService>
{
    protected override UserService CreateService()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        return new UserService(DbFactory, configuration);
    }

    [TestMethod]
    public async Task CheckUserFlagsAsync_NotFound()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var result = await Service.CheckUserFlagsAsync(user, UserFlags.BotAdmin);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckUserFlagsAsync_Found_NotAdmin()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        await DbContext.InitUserAsync(user, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        var result = await Service.CheckUserFlagsAsync(user, UserFlags.BotAdmin);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CheckUserFlagsAsync_Found_Admin()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var userEntity = Database.Entity.User.FromDiscord(user);
        userEntity.Flags |= (int)UserFlags.BotAdmin;

        await DbContext.Users.AddAsync(userEntity);
        await DbContext.SaveChangesAsync();

        var result = await Service.CheckUserFlagsAsync(user, UserFlags.BotAdmin);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CreateWebAdminLink_NotAdmin()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var result = await Service.CreateWebAdminLink(user, user);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateWebAdminLink_Admin()
    {
        var dcUser = new UserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).Build();
        var userEntity = Database.Entity.User.FromDiscord(dcUser);
        userEntity.Flags |= (int)UserFlags.WebAdmin;

        await DbContext.Users.AddAsync(userEntity);
        await DbContext.SaveChangesAsync();

        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var result = await Service.CreateWebAdminLink(dcUser, user);
        Assert.AreEqual("http://grillbot/370506820197810176", result);
    }

    [TestMethod]
    public void GetUserStateEmote_Offline()
        => GetUserStateEmote_Test(UserStatus.Offline, Emote.Parse("<:Offline:856875666842583040>"), "Offline");

    [TestMethod]
    public void GetUserStateEmote_Online()
        => GetUserStateEmote_Test(UserStatus.Online, Emote.Parse("<:Online:856875667379585034>"), "Online");

    [TestMethod]
    public void GetUserStateEmote_Idle()
        => GetUserStateEmote_Test(UserStatus.Idle, Emote.Parse("<:Idle:856879314997346344>"), "Nepřítomen");

    [TestMethod]
    public void GetUserStateEmote_AFK()
        => GetUserStateEmote_Test(UserStatus.AFK, Emote.Parse("<:Idle:856879314997346344>"), "Nepřítomen");

    [TestMethod]
    public void GetUserStateEmote_DoNotDisturb()
        => GetUserStateEmote_Test(UserStatus.DoNotDisturb, Emote.Parse("<:DoNotDisturb:856879762282774538>"), "Nerušit");

    [TestMethod]
    public void GetUserStateEmote_Invisible()
        => GetUserStateEmote_Test(UserStatus.Invisible, Emote.Parse("<:Offline:856875666842583040>"), "Offline");

    private void GetUserStateEmote_Test(UserStatus status, Emote expectedEmote, string expectedStatus)
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetStatus(status).Build();
        var result = Service.GetUserStateEmote(user, out var userStatus);

        Assert.AreEqual(expectedEmote, result);
        Assert.AreEqual(expectedStatus, userStatus);
    }
}
