using GrillBot.App.Services.Permissions;

namespace GrillBot.Tests.App.Services.Permissions.Models;

[TestClass]
public class PermsCheckResultTests
{
    [TestMethod]
    public void ToString_ContextCheckFail()
    {
        var result = new PermsCheckResult() { ContextCheck = false };
        Assert.IsFalse(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_IsAdmin()
    {
        var result = new PermsCheckResult() { IsAdmin = true };
        Assert.IsTrue(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_ChannelDisabled()
    {
        var result = new PermsCheckResult() { ChannelDisabled = true };
        Assert.IsFalse(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_ExplicitBan()
    {
        var result = new PermsCheckResult() { ExplicitBan = true };
        Assert.IsFalse(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_ExplicitAllow()
    {
        var result = new PermsCheckResult() { ExplicitAllow = true };
        Assert.IsTrue(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_BoosterAllowed()
    {
        var result = new PermsCheckResult() { BoosterAllowed = true };
        Assert.IsTrue(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_GuildPermissions()
    {
        var result = new PermsCheckResult() { GuildPermissions = true };
        Assert.IsTrue(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_ChannelPermissions()
    {
        var result = new PermsCheckResult() { ChannelPermissions = true };
        Assert.IsTrue(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_GuildPermissionsFail()
    {
        var result = new PermsCheckResult() { GuildPermissions = false };
        Assert.IsFalse(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_ChannelPermissionsFail()
    {
        var result = new PermsCheckResult() { ChannelPermissions = false };
        Assert.IsFalse(string.IsNullOrEmpty(result.ToString()));
    }

    [TestMethod]
    public void ToString_OtherFail()
    {
        var result = new PermsCheckResult();
        Assert.IsFalse(string.IsNullOrEmpty(result.ToString()));
    }
}
