using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using AuditLog.Enums;
using GrillBot.Core.Exceptions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.App.Managers.Points;
using AuditLog.Models.Events.Create;
using PointsService.Models.Users;
using GrillBot.Common.Extensions.Discord;
using PointsService.Models.Channels;
using GrillBot.Database.Enums;
using SearchingService.Models.Events;
using GrillBot.Core.Extensions;
using SearchingService.Models.Events.Users;
using GrillBot.Core.RabbitMQ.V2.Publisher;

namespace GrillBot.App.Actions.Api.V1.User;

public class UpdateUser : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private IDiscordClient DiscordClient { get; }

    private readonly PointsManager _pointsManager;
    private readonly IRabbitPublisher _rabbitPublisher;

    public UpdateUser(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, IDiscordClient discordClient, PointsManager pointsManager,
        IRabbitPublisher rabbitPublisher) : base(apiContext)
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

        using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserByIdAsync(id)
            ?? throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var before = user.Clone();
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;
        user.Flags = parameters.GetNewFlags(user.Flags);

        await repository.CommitAsync();
        await WriteToAuditLogAsync(before, user);
        await SyncPointsServiceAsync(id, parameters);
        await SyncSearchingServiceAsync(user);
        return ApiResult.Ok();
    }

    private async Task SyncPointsServiceAsync(ulong userId, UpdateUserParams parameters)
    {
        var guilds = await DiscordClient.GetGuildsAsync();

        foreach (var guild in guilds)
        {
            var user = await guild.GetUserAsync(userId);
            if (user is null) continue;

            var item = new UserSyncItem
            {
                Id = user.Id.ToString(),
                IsUser = user.IsUser(),
                PointsDisabled = parameters.PointsDisabled
            };

            await _pointsManager.PushSynchronizationAsync(guild, new[] { item }, Enumerable.Empty<ChannelSyncItem>());
        }
    }

    private async Task WriteToAuditLogAsync(Database.Entity.User before, Database.Entity.User after)
    {
        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.MemberUpdated, DateTime.UtcNow, null, userId)
        {
            MemberUpdated = new MemberUpdatedRequest
            {
                UserId = after.Id,
                Flags = new DiffRequest<int?>
                {
                    After = after.Flags,
                    Before = before.Flags
                },
                SelfUnverifyMinimalTime = new DiffRequest<string?>
                {
                    After = after.SelfUnverifyMinimalTime?.ToString("c"),
                    Before = before.SelfUnverifyMinimalTime?.ToString("c")
                },
                PointsDeactivated = new DiffRequest<bool?>
                {
                    After = after.HaveFlags(UserFlags.PointsDisabled),
                    Before = before.HaveFlags(UserFlags.PointsDisabled)
                }
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
    }

    private async Task SyncSearchingServiceAsync(Database.Entity.User user)
    {
        var guilds = await DiscordClient.GetGuildsAsync();
        var payload = new SynchronizationPayload();

        foreach (var guild in guilds)
        {
            var guildUser = await guild.GetUserAsync(user.Id.ToUlong());
            if (guildUser is null)
                continue;

            var isAdmin = user.HaveFlags(UserFlags.BotAdmin);
            var permissions = guildUser.GuildPermissions.ToList().Aggregate((prev, curr) => prev | curr);
            payload.Users.Add(new UserSynchronizationItem(guild.Id.ToString(), user.Id, isAdmin, permissions));
        }

        if (payload.Users.Count > 0)
            await _rabbitPublisher.PublishAsync(payload);
    }
}
