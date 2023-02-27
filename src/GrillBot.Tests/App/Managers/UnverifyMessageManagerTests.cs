using GrillBot.App.Managers;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Managers;

[TestClass]
public class UnverifyMessageManagerTests : TestBase<UnverifyMessageManager>
{
    protected override UnverifyMessageManager CreateInstance()
    {
        return new UnverifyMessageManager(TestServices.Texts.Value);
    }

    [TestMethod]
    public void CreateUnverifyMessageToChannel_Selfunverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true, "cs");
        var result = Instance.CreateUnverifyMessageToChannel(profile, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "04. 02. 2022 00:00:00");
    }

    [TestMethod]
    public void CreateUnverifyMessageToChannel_Unverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false, "cs") { Reason = "Duvod" };
        var result = Instance.CreateUnverifyMessageToChannel(profile, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "04. 02. 2022 00:00:00", "Duvod");
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Selfunverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true, "cs");
        var result = Instance.CreateUnverifyPmMessage(profile, guild, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-Guild-Name", "04. 02. 2022 00:00:00");
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Unverify()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false, "cs") { Reason = "Duvod" };
        var result = Instance.CreateUnverifyPmMessage(profile, guild, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-Guild-Name", "04. 02. 2022 00:00:00", "Duvod");
    }

    [TestMethod]
    public void CreateUpdatePmMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Instance.CreateUpdatePmMessage(guild, end, null, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-Guild-Name", "04. 02. 2022 00:00:00");
    }

    [TestMethod]
    public void CreateUpdatePmMessageWithReason()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Instance.CreateUpdatePmMessage(guild, end, "Reason", "cs");

        StringHelper.CheckTextParts(result, "GrillBot-Guild-Name", "04. 02. 2022 00:00:00", "Reason");
    }

    [TestMethod]
    public void CreateUpdateChannelMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Instance.CreateUpdateChannelMessage(guildUser, end, null, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "04. 02. 2022 00:00:00");
    }

    [TestMethod]
    public void CreateUpdateChannelMessageWithReason()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var result = Instance.CreateUpdateChannelMessage(guildUser, end, "Reason", "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "04. 02. 2022 00:00:00", "Reason");
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyPmMessage()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var result = Instance.CreateRemoveAccessManuallyPmMessage(guild, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-Guild-Name");
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyToChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var result = Instance.CreateRemoveAccessManuallyToChannel(guildUser, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234");
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyFailed()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var exception = new Exception("Test");
        var result = Instance.CreateRemoveAccessManuallyFailed(guildUser, exception, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234", "(Test)");
    }

    [TestMethod]
    public void CreateRemoveAccessUnverifyNotFound()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var result = Instance.CreateRemoveAccessUnverifyNotFound(guildUser, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234");
    }

    [TestMethod]
    public void CreateUnverifyFailedToChannel()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var result = Instance.CreateUnverifyFailedToChannel(guildUser, "cs");

        StringHelper.CheckTextParts(result, "GrillBot-User-Username#1234");
    }
}
