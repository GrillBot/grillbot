using GrillBot.App.Services;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services;

[TestClass]
public class SearchingServiceTests : ServiceTest<SearchingService>
{
    protected override SearchingService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService);
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();

        return new SearchingService(discordClient, dbFactory, messageCache);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_NoMessage()
    {
        await Service.CreateAsync(null, null, null, null);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_EmptyMessage()
    {
        var message = DataHelper.CreateMessage(content: "");
        await Service.CreateAsync(null, null, null, message);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_EmptyRegexMessage()
    {
        var message = DataHelper.CreateMessage(content: "$hledam");
        await Service.CreateAsync(null, null, null, message);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task CreateAsync_LongMessage()
    {
        var message = DataHelper.CreateMessage(content: new string('c', 5000));
        await Service.CreateAsync(null, null, null, message);
    }
}
