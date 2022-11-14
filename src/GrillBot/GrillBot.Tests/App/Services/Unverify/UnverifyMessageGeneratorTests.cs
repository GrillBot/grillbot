using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyMessageGeneratorTests
{
    [TestMethod]
    public void CreateUnverifyMessageToChannel_Selfunverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true, "cs");
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/UnverifyToChannelWithoutReason", "cs", "{0},{1}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUnverifyMessageToChannel(profile, "cs");

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
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/UnverifyToChannelWithReason", "cs", "{0},{1},{2}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUnverifyMessageToChannel(profile, "cs");

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
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/PrivateUnverifyWithoutReason", "cs", "{0},{1}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUnverifyPmMessage(profile, guild, "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00", result);
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Unverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false, "cs") { Reason = "Duvod" };
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/PrivateUnverifyWithReason", "cs", "{0},{1},{2}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUnverifyPmMessage(profile, guild, "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00,Duvod", result);
    }

    [TestMethod]
    public void CreateUpdatePmMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var end = new DateTime(2022, 02, 04);
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/PrivateUpdate", "cs", "{0},{1}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUpdatePmMessage(guild, end, "cs");

        Assert.AreEqual("GrillBot-Guild-Name,04. 02. 2022 00:00:00", result);
    }

    [TestMethod]
    public void CreateUpdateChannelMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/UpdateToChannel", "cs", "{0},{1}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUpdateChannelMessage(guildUser, end, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234,04. 02. 2022 00:00:00", result);
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyPmMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/PrivateManuallyRemovedUnverify", "cs", "{0}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateRemoveAccessManuallyPmMessage(guild, "cs");

        Assert.AreEqual("GrillBot-Guild-Name", result);
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyToChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/ManuallyRemoveToChannel", "cs", "{0}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateRemoveAccessManuallyToChannel(guildUser, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234", result);
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyFailed()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var exception = new Exception("Test");
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/ManuallyRemoveFailed", "cs", "{0}({1})")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateRemoveAccessManuallyFailed(guildUser, exception, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234(Test)", result);
    }

    [TestMethod]
    public void CreateRemoveAccessUnverifyNotFound()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/RemoveAccessUnverifyNotFound", "cs", "{0}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateRemoveAccessUnverifyNotFound(guildUser, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234", result);
    }

    [TestMethod]
    public void CreateUnverifyFailedToChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var texts = new TextsBuilder()
            .AddText("Unverify/Message/UnverifyFailedToChannel", "cs", "{0}")
            .Build();
        var result = new UnverifyMessageGenerator(texts).CreateUnverifyFailedToChannel(guildUser, "cs");

        Assert.AreEqual("GrillBot-User-Username#1234", result);
    }
}
