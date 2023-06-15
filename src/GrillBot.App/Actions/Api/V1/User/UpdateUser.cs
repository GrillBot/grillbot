using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.User;

public class UpdateUser : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private PointsHelper PointsHelper { get; }
    private IDiscordClient DiscordClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public UpdateUser(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, PointsHelper pointsHelper, IDiscordClient discordClient,
        IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        PointsHelper = pointsHelper;
        DiscordClient = discordClient;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task ProcessAsync(ulong id, UpdateUserParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserByIdAsync(id);

        if (user == null)
            throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var before = user.Clone();
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;
        user.Flags = parameters.GetNewFlags(user.Flags);

        await repository.CommitAsync();
        await WriteToAuditLogAsync(before, user);
        await TrySyncPointsService(before, user);
    }

    private async Task TrySyncPointsService(Database.Entity.User before, Database.Entity.User after)
    {
        if (before.HaveFlags(UserFlags.PointsDisabled) == after.HaveFlags(UserFlags.PointsDisabled))
            return;

        var guilds = await DiscordClient.GetGuildsAsync();
        foreach (var guild in guilds)
        {
            var user = await guild.GetUserAsync(after.Id.ToUlong());
            if (user is null) continue;

            await PointsHelper.SyncDataWithServiceAsync(guild, new[] { user }, Enumerable.Empty<IGuildChannel>());
        }
    }

    private async Task WriteToAuditLogAsync(Database.Entity.User before, Database.Entity.User after)
    {
        var logRequest = new LogRequest
        {
            Type = LogType.MemberUpdated,
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
            },
            UserId = ApiContext.GetUserId().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
