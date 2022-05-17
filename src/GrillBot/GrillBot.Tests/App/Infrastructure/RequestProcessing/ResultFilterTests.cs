using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace GrillBot.Tests.App.Infrastructure.RequestProcessing;

[TestClass]
public class ResultFilterTests : ActionFilterTest<ResultFilter>
{
    private ApiRequest ApiRequest { get; set; }

    protected override bool CanInitProvider() => false;

    protected override Controller CreateController(IServiceProvider provider)
        => new AuthController(null);

    protected override ResultFilter CreateFilter()
    {
        ApiRequest = new ApiRequest();

        var user = new UserBuilder()
            .SetUsername(Consts.Username)
            .SetDiscriminator(Consts.Discriminator)
            .SetId(0)
            .Build();

        var client = new ClientBuilder()
            .SetGetUserAction(user)
            .Build();

        var discordClient = DiscordHelper.CreateClient();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var storage = FileStorageHelper.Create(configuration);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, storage, initializationService);

        return new ResultFilter(ApiRequest, auditLogService, client);
    }

    [TestMethod]
    public async Task OnResultExecutionAsync()
    {
        var context = GetContext(new OkResult());
        await Filter.OnResultExecutionAsync(context, GetResultDelegate());

        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.EndAt);
    }
}
