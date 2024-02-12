using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Request.Search;
using GrillBot.Core.Services.AuditLog.Models.Response.Search;
using GrillBot.Core.Services.UserMeasures.Models.Events;
using GrillBot.Data.Models.Unverify;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Text.Json;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class UserMeasuresMigrationJob : Job
{
    private IRabbitMQPublisher RabbitMQ { get; }
    private IAuditLogServiceClient AuditLogService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public UserMeasuresMigrationJob(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        RabbitMQ = serviceProvider.GetRequiredService<IRabbitMQPublisher>();
        AuditLogService = serviceProvider.GetRequiredService<IAuditLogServiceClient>();
        DatabaseBuilder = serviceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await MigrateUnverifyAsync();
        await MigrateWarningsAsync();
    }

    private async Task MigrateWarningsAsync()
    {
        var request = new SearchRequest
        {
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            },
            ShowTypes = new List<Core.Services.AuditLog.Enums.LogType> { Core.Services.AuditLog.Enums.LogType.MemberWarning }
        };

        var warnings = await AuditLogService.SearchItemsAsync(request);
        warnings.ValidationErrors.AggregateAndThrow();

        var serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        foreach (var item in warnings.Response!.Data)
        {
            var payload = new MemberWarningPayload
            {
                CreatedAt = item.CreatedAt.ToUniversalTime(),
                GuildId = item.GuildId!,
                ModeratorId = item.UserId!
            };

            var preview = ((JsonElement)item.Preview!).Deserialize<MemberWarningPreview>(serializationOptions)!;
            payload.Reason = preview.Reason;
            payload.TargetUserId = preview.TargetId;

            await RabbitMQ.PublishAsync(MemberWarningPayload.QueueName, payload);
        }
    }

    private async Task MigrateUnverifyAsync()
    {
        var parameters = new Data.Models.API.Unverify.UnverifyLogParams
        {
            Operation = Database.Enums.UnverifyOperation.Unverify,
            Pagination =
            {
                Page = 0,
                PageSize = int.MaxValue
            }
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var logs = await repository.Unverify.GetLogsAsync(parameters, parameters.Pagination, new List<string>());

        foreach (var log in logs.Data)
        {
            var logData = JsonConvert.DeserializeObject<UnverifyLogSet>(log.Data)!;
            var payload = new UnverifyPayload
            {
                CreatedAt = logData.Start.ToUniversalTime(),
                EndAt = logData.End.ToUniversalTime(),
                GuildId = log.GuildId,
                LogSetId = log.Id,
                ModeratorId = log.FromUserId,
                Reason = logData.Reason!,
                TargetUserId = log.ToUserId
            };

            await RabbitMQ.PublishAsync(UnverifyPayload.QueueName, payload);
        }
    }
}
