using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.User;

public class UpdateUser : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private ITextsManager Texts { get; }
    private PointsHelper PointsHelper { get; }
    private IDiscordClient DiscordClient { get; }

    public UpdateUser(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriteManager auditLogWriteManager, ITextsManager texts, PointsHelper pointsHelper,
        IDiscordClient discordClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriteManager = auditLogWriteManager;
        Texts = texts;
        PointsHelper = pointsHelper;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync(ulong id, UpdateUserParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserByIdAsync(id);

        if (user == null)
            throw new NotFoundException(Texts["User/NotFound", ApiContext.Language]);

        var before = user.Clone();
        user.Note = parameters.Note;
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;
        user.Flags = parameters.GetNewFlags(user.Flags);

        var auditLogItem = new AuditLogDataWrapper(AuditLogItemType.MemberUpdated, new MemberUpdatedData(before, user), processedUser: ApiContext.LoggedUser);
        await AuditLogWriteManager.StoreAsync(auditLogItem);

        await repository.CommitAsync();
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
}
