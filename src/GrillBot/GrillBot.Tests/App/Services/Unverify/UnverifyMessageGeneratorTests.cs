using Discord;
using GrillBot.Data.Services.Unverify;
using GrillBot.Data.Models.Unverify;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace GrillBot.Tests.App.Services.Unverify
{
    [TestClass]
    public class UnverifyMessageGeneratorTests
    {
        [TestMethod]
        public void CreateUnverifyMessageToChannel()
        {
            const string expected = "Dočasné odebrání přístupu pro uživatele **User** bylo dokončeno. Přístup bude navrácen **02. 07. 2021 15:30:25**. Důvod: Test";

            var destination = DiscordHelpers.CreateGuildUserMock(0, null, "User");
            var end = new DateTime(2021, 07, 02, 15, 30, 25);
            var profile = new UnverifyUserProfile(destination.Object, DateTime.MinValue, end, false) { Reason = "Test" };

            var result = UnverifyMessageGenerator.CreateUnverifyMessageToChannel(profile);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateUnverifyPMMessage()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("Guild");

            const string expected = "Byly ti dočasně odebrány všechny práva na serveru **Guild**. Přístup ti bude navrácen **02. 07. 2021 15:30:25**. Důvod: Test";

            var destination = DiscordHelpers.CreateGuildUserMock(0, null, "User");

            var end = new DateTime(2021, 07, 02, 15, 30, 25);
            var profile = new UnverifyUserProfile(destination.Object, DateTime.MinValue, end, false) { Reason = "Test" };

            var result = UnverifyMessageGenerator.CreateUnverifyPMMessage(profile, guild.Object);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateUpdatePMMessage()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("Guild");

            const string expected = "Byl ti aktualizován čas pro odebrání práv na serveru **Guild**. Přístup ti bude navrácen **02. 07. 2021 15:30:25**.";

            var end = new DateTime(2021, 07, 02, 15, 30, 25);
            var result = UnverifyMessageGenerator.CreateUpdatePMMessage(guild.Object, end);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateUpdateChannelMessage()
        {
            var user = DiscordHelpers.CreateGuildUserMock(0, null, "User");

            const string expected = "Reset konce odebrání přístupu pro uživatele **User** byl aktualizován.\nPřístup bude navrácen **02. 07. 2021 15:30:25**";

            var end = new DateTime(2021, 07, 02, 15, 30, 25);
            var result = UnverifyMessageGenerator.CreateUpdateChannelMessage(user.Object, end);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateRemoveAccessManuallyPMMessage()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Name).Returns("Guild");

            const string expected = "Byl ti předčasně vrácen přístup na serveru **Guild**";
            var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyPMMessage(guild.Object);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateRemoveAccessManuallyToChannel()
        {
            var user = DiscordHelpers.CreateGuildUserMock(0, null, "User");

            const string expected = "Předčasné vrácení přístupu pro uživatele **User** bylo dokončeno.";
            var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyToChannel(user.Object);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateRemoveAccessManuallyFailed()
        {
            var user = DiscordHelpers.CreateGuildUserMock(0, "U", "User");
            user.Setup(o => o.Discriminator).Returns("1234");
            var exception = new Exception("Test");

            const string expected = "Předčasné vrácení přístupu pro uživatele **User (U#1234)** selhalo. (Test)";
            var result = UnverifyMessageGenerator.CreateRemoveAccessManuallyFailed(user.Object, exception);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateRemoveAccessUnverifyNotFound()
        {
            var user = DiscordHelpers.CreateGuildUserMock(0, null, "User");

            const string expected = "Předčasné vrácení přístupu pro uživatele **User** nelze provést. Unverify nebylo nalezeno.";
            var result = UnverifyMessageGenerator.CreateRemoveAccessUnverifyNotFound(user.Object);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateUnverifyFailedToChannel()
        {
            var user = DiscordHelpers.CreateGuildUserMock(0, null, "User");

            const string expected = "Dočasné odebrání přístupu pro uživatele **User** se nezdařilo. Uživatel byl obnoven do původního stavu.";
            var result = UnverifyMessageGenerator.CreateUnverifyFailedToChannel(user.Object);
            Assert.AreEqual(expected, result);
        }
    }
}
