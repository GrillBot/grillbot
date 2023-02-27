using Discord.WebSocket;
using GrillBot.App.Actions.Api.V1.System;
using GrillBot.App.Controllers;
using GrillBot.App.Jobs;
using GrillBot.App.Managers;
using GrillBot.App.Modules.Interactions;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.System;

[TestClass]
public class GetDashboardTests : ApiActionTest<GetDashboard>
{
    protected override GetDashboard CreateInstance()
    {
        var client = new ClientBuilder().Build();
        var initManager = new InitManager(TestServices.LoggerFactory.Value);
        initManager.Set(true);
        var discordClient = new DiscordSocketClient();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var logging = new LoggingManager(discordClient, interactionService, TestServices.Provider.Value);

        return new GetDashboard(ApiRequestContext, TestServices.TestingEnvironment.Value, client, initManager, TestServices.CounterManager.Value,
            DatabaseBuilder, logging, TestServices.Graphics.Value, TestServices.RubbergodServiceClient.Value);
    }

    private async Task InitDataAsync()
    {
        for (var i = 0; i < 15; i++)
        {
            await Repository.AddAsync(CreateLogItem(new ApiRequest
            {
                ControllerName = nameof(UsersController),
                ActionName = nameof(UsersController.GetTodayBirthdayInfoAsync),
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddMilliseconds(10),
                Method = "GET",
                TemplatePath = "/user/birthday",
                Path = "/user/birthday",
                LoggedUserRole = null,
                StatusCode = "200 OK",
                Parameters = new Dictionary<string, string>(),
                Language = "cs",
                ApiGroupName = "V2"
            }, AuditLogItemType.Api));
        }

        for (var i = 0; i < 15; i++)
        {
            await Repository.AddAsync(CreateLogItem(new ApiRequest
            {
                ControllerName = nameof(AuthController),
                ActionName = nameof(AuthController.GetRedirectLink),
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddMilliseconds(10),
                Method = "GET",
                TemplatePath = "/auth/link",
                Path = "/auth/link",
                LoggedUserRole = null,
                StatusCode = "200 OK",
                Parameters = new Dictionary<string, string>(),
                Language = "cs",
                ApiGroupName = "V1"
            }, AuditLogItemType.Api));
        }

        await Repository.AddCollectionAsync(new[]
        {
            CreateLogItem(new ApiRequest(), AuditLogItemType.Api),
            CreateLogItem(new ApiRequest
            {
                ControllerName = nameof(SystemController),
                ActionName = nameof(SystemController.GetDashboardAsync),
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddMilliseconds(10),
                Method = "GET",
                TemplatePath = "/system/dashboard",
                Path = "/system/dashboard",
                LoggedUserRole = "Admin",
                StatusCode = "200 OK",
                Parameters = new Dictionary<string, string>(),
                Language = "cs",
                ApiGroupName = "V1"
            }, AuditLogItemType.Api),
            CreateLogItem(new ApiRequest
            {
                ControllerName = nameof(UsersController),
                ActionName = nameof(UsersController.GetTodayBirthdayInfoAsync),
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddMilliseconds(10),
                Method = "GET",
                TemplatePath = "/user/birthday",
                Path = "/user/birthday",
                LoggedUserRole = null,
                StatusCode = "200 OK",
                Parameters = new Dictionary<string, string>(),
                Language = "cs",
                ApiGroupName = "V2"
            }, AuditLogItemType.Api),
            CreateLogItem(new JobExecutionData
            {
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddMilliseconds(100),
                Result = "Done",
                JobName = nameof(AuditLogClearingJob),
                WasError = false
            }, AuditLogItemType.JobCompleted),
            CreateLogItem(new InteractionCommandExecuted
            {
                Parameters = new List<InteractionCommandParameter>(),
                Duration = 100,
                Exception = null,
                Locale = "cs",
                Name = "/points board",
                CommandError = null,
                ErrorReason = null,
                HasResponded = true,
                IsSuccess = true,
                MethodName = nameof(PointsModule.GetPointsBoardAsync),
                ModuleName = nameof(PointsModule),
                IsValidToken = true
            }, AuditLogItemType.InteractionCommand)
        });

        await Repository.CommitAsync();
    }

    private static Database.Entity.AuditLogItem CreateLogItem(object data, AuditLogItemType type)
    {
        return new Database.Entity.AuditLogItem
        {
            Data = JsonConvert.SerializeObject(data, AuditLogWriteManager.SerializerSettings),
            Type = type,
            CreatedAt = DateTime.Now
        };
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        await InitDataAsync();
        var result = await Instance.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsDevelopment);
        Assert.IsTrue(result.IsActive);
        Assert.IsNotNull(result.ActiveOperations);
        Assert.IsNotNull(result.OperationStats);
        Assert.IsNotNull(result.TodayAvgTimes);
        Assert.IsNotNull(result.InternalApiRequests);
        Assert.IsNotNull(result.PublicApiRequests);
        Assert.IsNotNull(result.Jobs);
        Assert.IsNotNull(result.Commands);
        Assert.IsNotNull(result.Services);
    }
}
