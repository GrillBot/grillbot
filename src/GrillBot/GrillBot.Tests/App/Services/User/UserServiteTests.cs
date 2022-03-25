using Discord;
using GrillBot.App.Services.User;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using System.Linq;

namespace GrillBot.Tests.App.Services.User;

[TestClass]
public class UserServiteTests : ServiceTest<UserService>
{
    protected override UserService CreateService()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var discordClient = DiscordHelper.CreateClient();

        return new UserService(DbFactory, configuration, discordClient);
    }

    public override void Cleanup()
    {
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task IsUserBotAdmin_NotFound()
    {
        var dcUser = DataHelper.CreateDiscordUser();
        var result = await Service.IsUserBotAdminAsync(dcUser);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsUserBotAdmin_Found_NotAdmin()
    {
        var dcUser = DataHelper.CreateDiscordUser(id: 654321);
        await DbContext.InitUserAsync(dcUser, CancellationToken.None);
        await DbContext.SaveChangesAsync();

        var result = await Service.IsUserBotAdminAsync(dcUser);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsUserBotAdmin_Found_Admin()
    {
        var dcUser = DataHelper.CreateDiscordUser(id: 1234556);
        var userEntity = Database.Entity.User.FromDiscord(dcUser);
        userEntity.Flags |= (int)UserFlags.BotAdmin;

        await DbContext.Users.AddAsync(userEntity);
        await DbContext.SaveChangesAsync();

        var result = await Service.IsUserBotAdminAsync(dcUser);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CreateWebAdminLink_NotAdmin()
    {
        var user = DataHelper.CreateDiscordUser();

        var result = await Service.CreateWebAdminLink(user, user);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateWebAdminLink_Admin()
    {
        var dcUser = DataHelper.CreateDiscordUser(id: 12345566);
        var userEntity = Database.Entity.User.FromDiscord(dcUser);
        userEntity.Flags |= (int)UserFlags.WebAdmin;

        await DbContext.Users.AddAsync(userEntity);
        await DbContext.SaveChangesAsync();

        var user = DataHelper.CreateDiscordUser();

        var result = await Service.CreateWebAdminLink(dcUser, user);
        Assert.AreEqual("http://grillbot/12345", result);
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
        var user = DataHelper.CreateDiscordUser(userStatus: status);
        var result = Service.GetUserStateEmote(user, out var userStatus);

        Assert.AreEqual(expectedEmote, result);
        Assert.AreEqual(expectedStatus, userStatus);
    }
}
