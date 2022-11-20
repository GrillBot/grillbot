using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyMessageGeneratorTests : ServiceTest<UnverifyMessageGenerator>
{
    protected override UnverifyMessageGenerator CreateService()
    {
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/UnverifyToChannelWithoutReason", "cs", "{0},{1}")
            .AddText("Unverify/Message/UnverifyToChannelWithReason", "cs", "{0},{1},{2}")
            .AddText("Unverify/Message/PrivateUnverifyWithoutReason", "cs", "{0},{1}")
            .AddText("Unverify/Message/PrivateUnverifyWithReason", "cs", "{0},{1},{2}")
            .AddText("Unverify/Message/PrivateUpdate", "cs", "{0},{1}")
            .AddText("Unverify/Message/PrivateUpdateWithReason", "cs", "{0},{1},{2}")
            .AddText("Unverify/Message/UpdateToChannel", "cs", "{0},{1}")
            .AddText("Unverify/Message/UpdateToChannelWithReason", "cs", "{0},{1},{2}")
            .AddText("Unverify/Message/PrivateManuallyRemovedUnverify", "cs", "{0}")
            .AddText("Unverify/Message/ManuallyRemoveToChannel", "cs", "{0}")
            .AddText("Unverify/Message/ManuallyRemoveFailed", "cs", "{0}({1})")
            .AddText("Unverify/Message/RemoveAccessUnverifyNotFound", "cs", "{0}")
            .AddText("Unverify/Message/UnverifyFailedToChannel", "cs", "{0}")
            .Build();
    
        return new UnverifyMessageGenerator(texts);
    }

    [TestMethod]
    public void CreateUnverifyMessageToChannel_Selfunverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true, "cs");
        var result = Service.CreateUnverifyMessageToChannel(profile, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234,04. 02. 2022 00:00:00", result);
    }

    [TestMethod]
    public void CreateUnverifyMessageToChannel_Unverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false, "cs") { Reason = "Duvod" };
        var result = Service.CreateUnverifyMessageToChannel(profile, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234,04. 02. 2022 00:00:00,Duvod", result);
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Selfunverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true, "cs");
        var result = Service.CreateUnverifyPmMessage(profile, guild, "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00", result);
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Unverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false, "cs") { Reason = "Duvod" };
        var result = Service.CreateUnverifyPmMessage(profile, guild, "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00,Duvod", result);
    }

    [TestMethod]
    public void CreateUpdatePmMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Service.CreateUpdatePmMessage(guild, end, null, "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00", result);
    }

    [TestMethod]
    public void CreateUpdatePmMessageWithReason()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Service.CreateUpdatePmMessage(guild, end, "Reason", "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00,Reason", result);
    }

    [TestMethod]
    public void CreateUpdateChannelMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Service.CreateUpdateChannelMessage(guildUser, end, null, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234,04. 02. 2022 00:00:00", result);
    }
    
    [TestMethod]
    public void CreateUpdateChannelMessageWithReason()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Service.CreateUpdateChannelMessage(guildUser, end, "Reason", "cs");

        Assert.AreEqual("GrillBot-User-Username#1234,04. 02. 2022 00:00:00,Reason", result);
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyPmMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var result = Service.CreateRemoveAccessManuallyPmMessage(guild, "cs");

        Assert.AreEqual("GrillBot-Guild-Name", result);
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyToChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var result = Service.CreateRemoveAccessManuallyToChannel(guildUser, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234", result);
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyFailed()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var exception = new Exception("Test");
        var result = Service.CreateRemoveAccessManuallyFailed(guildUser, exception, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234(Test)", result);
    }

    [TestMethod]
    public void CreateRemoveAccessUnverifyNotFound()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var result = Service.CreateRemoveAccessUnverifyNotFound(guildUser, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234", result);
    }

    [TestMethod]
    public void CreateUnverifyFailedToChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var result = Service.CreateUnverifyFailedToChannel(guildUser, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234", result);
    }
}
