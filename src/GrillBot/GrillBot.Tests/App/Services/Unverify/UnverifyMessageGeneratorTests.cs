using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyMessageGeneratorTests
{
    [TestMethod]
    public void CreateUnverifyMessageToChannel_Selfunverify()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true);
        var result = UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

        Assert.AreEqual(
            "Dočasné odebrání přístupu pro uživatele **GrillBot-User-Username#1234** bylo dokončeno. Přístup bude navrácen **04. 02. 2022 00:00:00**. ",
            result
        );
    }

    [TestMethod]
    public void CreateUnverifyMessageToChannel_Unverify()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false) { Reason = "Duvod" };
        var result = UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

        Assert.AreEqual(
            "Dočasné odebrání přístupu pro uživatele **GrillBot-User-Username#1234** bylo dokončeno. Přístup bude navrácen **04. 02. 2022 00:00:00**. Důvod: Duvod",
            result
        );
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Selfunverify()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true);
        var result = UnverifyMessageGenerator.CreateUnverifyPmMessage(profile, guild);

        Assert.AreEqual(
            "Byly ti dočasně odebrány všechny práva na serveru **GrillBot-Guild-Name**. Přístup ti bude navrácen **04. 02. 2022 00:00:00**. ",
            result
        );
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Unverify()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false) { Reason = "Duvod" };
        var result = UnverifyMessageGenerator.CreateUnverifyPmMessage(profile, guild);

        Assert.AreEqual(
            "Byly ti dočasně odebrány všechny práva na serveru **GrillBot-Guild-Name**. Přístup ti bude navrácen **04. 02. 2022 00:00:00**. Důvod: Duvod",
            result
        );
    }

    [TestMethod]
    public void CreateUpdatePMMessage()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var end = new DateTime(2022, 02, 04);
        var result = UnverifyMessageGenerator.CreateUpdatePmMessage(guild, end);

        Assert.AreEqual(
            "Byl ti aktualizován čas pro odebrání práv na serveru **GrillBot-Guild-Name**. Přístup ti bude navrácen **04. 02. 2022 00:00:00**.",
            result
        );
    }

    [TestMethod]
    public void CreateUpdateChannelMessage()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var end = new DateTime(2022, 02, 04);
        var result = UnverifyMessageGenerator.CreateUpdateChannelMessage(guildUser, end);

        Assert.AreEqual(
            "Reset konce odebrání přístupu pro uživatele **GrillBot-User-Username#1234** byl aktualizován.\nPřístup bude navrácen **04. 02. 2022 00:00:00**",
            result
        );
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyPMMessage()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyPmMessage(guild);

        Assert.AreEqual(
           "Byl ti předčasně vrácen přístup na serveru **GrillBot-Guild-Name**",
           result
        );
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyToChannel()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyToChannel(guildUser);

        Assert.AreEqual(
           "Předčasné vrácení přístupu pro uživatele **GrillBot-User-Username#1234** bylo dokončeno.",
           result
        );
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyFailed()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var exception = new Exception("Test");
        var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyFailed(guildUser, exception);

        Assert.AreEqual("Předčasné vrácení přístupu pro uživatele **GrillBot-User-Username#1234** selhalo. (Test)", result);
    }

    [TestMethod]
    public void CreateRemoveAccessUnverifyNotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var result = UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(guildUser);

        Assert.AreEqual("Předčasné vrácení přístupu pro uživatele **GrillBot-User-Username#1234** nelze provést. Unverify nebylo nalezeno.", result);
    }

    [TestMethod]
    public void CreateUnverifyFailedToChannel()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetGuild(guild).Build();
        var result = UnverifyMessageGenerator.CreateUnverifyFailedToChannel(guildUser);

        Assert.AreEqual("Dočasné odebrání přístupu pro uživatele **GrillBot-User-Username#1234** se nezdařilo. Uživatel byl obnoven do původního stavu.", result);
    }
}
