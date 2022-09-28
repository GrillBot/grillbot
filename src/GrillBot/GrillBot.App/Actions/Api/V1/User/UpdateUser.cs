using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.User;

public class UpdateUser : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private ITextsManager Texts { get; }

    public UpdateUser(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, AuditLogWriter auditLogWriter, ITextsManager texts) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        AuditLogWriter = auditLogWriter;
        Texts = texts;
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
        await AuditLogWriter.StoreAsync(auditLogItem);

        await repository.CommitAsync();
    }
}
