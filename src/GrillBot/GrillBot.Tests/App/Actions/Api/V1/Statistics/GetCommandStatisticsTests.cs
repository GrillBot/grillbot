using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetCommandStatisticsTests : ApiActionTest<GetCommandStatistics>
{
    protected override GetCommandStatistics CreateAction()
    {
        return new GetCommandStatistics(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddCollectionAsync(new[]
        {
            CreateLogItem(new InteractionCommandExecuted { Name = "Inter", ModuleName = "Module", MethodName = "Method" }, true),
            CreateLogItem(new CommandExecution { Command = "Command" }, false)
        });
        await Repository.CommitAsync();
    }

    private static Database.Entity.AuditLogItem CreateLogItem(object data, bool isInteraction)
    {
        return new Database.Entity.AuditLogItem
        {
            Data = JsonConvert.SerializeObject(data, AuditLogWriter.SerializerSettings),
            Type = !isInteraction ? AuditLogItemType.Command : AuditLogItemType.InteractionCommand,
            CreatedAt = DateTime.Now,
            ProcessedUserId = Consts.UserId.ToString()
        };
    }

    [TestMethod]
    public async Task ProcessTextCommandsAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessTextCommandsAsync();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessInteractionsAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessInteractionsAsync();
        Assert.AreEqual(1, result.Count);
    }
}
