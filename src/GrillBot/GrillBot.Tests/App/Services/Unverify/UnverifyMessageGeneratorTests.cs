using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.Unverify;
using System;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class UnverifyMessageGeneratorTests
{
    [TestMethod]
    public void CreateUnverifyMessageToChannel_Selfunverify()
    {
        var toUser = DataHelper.CreateGuildUser();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true);
        var result = UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

        Assert.AreEqual(
            "Dočasné odebrání přístupu pro uživatele **User#1111** bylo dokončeno. Přístup bude navrácen **04. 02. 2022 00:00:00**. ",
            result
        );
    }

    [TestMethod]
    public void CreateUnverifyMessageToChannel_Unverify()
    {
        var toUser = DataHelper.CreateGuildUser();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false) { Reason = "Duvod" };
        var result = UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);

        Assert.AreEqual(
            "Dočasné odebrání přístupu pro uživatele **User#1111** bylo dokončeno. Přístup bude navrácen **04. 02. 2022 00:00:00**. Důvod: Duvod",
            result
        );
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Selfunverify()
    {
        var toUser = DataHelper.CreateGuildUser();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, true);
        var guild = DataHelper.CreateGuild();
        var result = UnverifyMessageGenerator.CreateUnverifyPMMessage(profile, guild);

        Assert.AreEqual(
            "Byly ti dočasně odebrány všechny práva na serveru **Guild**. Přístup ti bude navrácen **04. 02. 2022 00:00:00**. ",
            result
        );
    }

    [TestMethod]
    public void CreateUnverifyPMMessage_Unverify()
    {
        var toUser = DataHelper.CreateGuildUser();
        var end = new DateTime(2022, 02, 04);
        var profile = new UnverifyUserProfile(toUser, DateTime.MinValue, end, false) { Reason = "Duvod" };
        var guild = DataHelper.CreateGuild();
        var result = UnverifyMessageGenerator.CreateUnverifyPMMessage(profile, guild);

        Assert.AreEqual(
            "Byly ti dočasně odebrány všechny práva na serveru **Guild**. Přístup ti bude navrácen **04. 02. 2022 00:00:00**. Důvod: Duvod",
            result
        );
    }

    [TestMethod]
    public void CreateUpdatePMMessage()
    {
        var guild = DataHelper.CreateGuild();
        var end = new DateTime(2022, 02, 04);
        var result = UnverifyMessageGenerator.CreateUpdatePMMessage(guild, end);

        Assert.AreEqual(
            "Byl ti aktualizován čas pro odebrání práv na serveru **Guild**. Přístup ti bude navrácen **04. 02. 2022 00:00:00**.",
            result
        );
    }

    [TestMethod]
    public void CreateUpdateChannelMessage()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var end = new DateTime(2022, 02, 04);
        var result = UnverifyMessageGenerator.CreateUpdateChannelMessage(guildUser, end);

        Assert.AreEqual(
            "Reset konce odebrání přístupu pro uživatele **User#1111** byl aktualizován.\nPřístup bude navrácen **04. 02. 2022 00:00:00**",
            result
        );
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyPMMessage()
    {
        var guild = DataHelper.CreateGuild();
        var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyPMMessage(guild);

        Assert.AreEqual(
           "Byl ti předčasně vrácen přístup na serveru **Guild**",
           result
        );
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyToChannel()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyToChannel(guildUser);

        Assert.AreEqual(
           "Předčasné vrácení přístupu pro uživatele **User#1111** bylo dokončeno.",
           result
        );
    }

    [TestMethod]
    public void CreateRemoveAccessManuallyFailed()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var exception = new Exception("Test");
        var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyFailed(guildUser, exception);

        Assert.AreEqual("Předčasné vrácení přístupu pro uživatele **User#1111** selhalo. (Test)", result);
    }

    [TestMethod]
    public void CreateRemoveAccessUnverifyNotFound()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var result = UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(guildUser);

        Assert.AreEqual("Předčasné vrácení přístupu pro uživatele **User#1111** nelze provést. Unverify nebylo nalezeno.", result);
    }

    [TestMethod]
    public void CreateUnverifyFailedToChannel()
    {
        var guildUser = DataHelper.CreateGuildUser();
        var result = UnverifyMessageGenerator.CreateUnverifyFailedToChannel(guildUser);

        Assert.AreEqual("Dočasné odebrání přístupu pro uživatele **User#1111** se nezdařilo. Uživatel byl obnoven do původního stavu.", result);
    }
}
