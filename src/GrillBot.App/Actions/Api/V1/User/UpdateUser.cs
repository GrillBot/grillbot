using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.App.Managers.Points;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Actions.Api.V1.User;

public class UpdateUser : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private IDiscordClient DiscordClient { get; }

    private readonly PointsManager _pointsManager;
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public UpdateUser(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IDiscordClient discordClient, PointsManager pointsManager,
        IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        DiscordClient = discordClient;
        _pointsManager = pointsManager;
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (ulong)Parameters[0]!;
        var parameters = (UpdateUserParams)Parameters[1]!;

        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserByIdAsync(id)
            ?? throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var before = user.Clone();
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;
        user.Flags = parameters.GetNewFlags(user.Flags);

        await repository.CommitAsync();
        await WriteToAuditLogAsync(before, user);
        await TrySyncPointsService(before, user);
        return ApiResult.Ok();
    }

    private async Task TrySyncPointsService(Database.Entity.User before, Database.Entity.User after)
    {
        if (before.HaveFlags(UserFlags.PointsDisabled) == after.HaveFlags(UserFlags.PointsDisabled))
            return;

        var guilds = await DiscordClient.GetGuildsAsync();
        foreach (var guild in guilds)
        {
            var user = await guild.GetUserAsync(after.Id.ToUlong());
            if (user is not null)
                await _pointsManager.PushSynchronizationAsync(guild, user);
        }
    }

    private async Task WriteToAuditLogAsync(Database.Entity.User before, Database.Entity.User after)
    {
        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.MemberUpdated, DateTime.UtcNow, null, userId)
        {
            MemberUpdated = new MemberUpdatedRequest
            {
                Flags = new DiffRequest<int?>
                {
                    After = after.Flags,
                    Before = before.Flags
                },
                SelfUnverifyMinimalTime = new DiffRequest<string?>
                {
                    After = before.SelfUnverifyMinimalTime?.ToString("c"),
                    Before = after.SelfUnverifyMinimalTime?.ToString("c")
                },
                UserId = after.Id
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
    }
}
