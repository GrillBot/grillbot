using GrillBot.App.Helpers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Commands.Reminder;

public class CopyRemind : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }
    private CreateRemind CreateRemind { get; }

    public CopyRemind(GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts, CreateRemind createRemind)
    {
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        CreateRemind = createRemind;
    }

    public async Task ProcessAsync(long originalRemindId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var original = await repository.Remind.FindRemindByIdAsync(originalRemindId);

        if (original == null)
            throw new NotFoundException(Texts["RemindModule/Copy/RemindNotFound", Locale]);

        if (original.FromUserId == Context.User.Id.ToString() && original.ToUserId == Context.User.Id.ToString())
            throw new ValidationException(Texts["RemindModule/Copy/SelfCopy", Locale]);

        if (!string.IsNullOrEmpty(original.RemindMessageId))
        {
            if (original.RemindMessageId == RemindHelper.NotSentRemind)
                throw new ValidationException(Texts["RemindModule/Copy/WasCancelled", Locale]);

            throw new ValidationException(Texts["RemindModule/Copy/WasSent", Locale]);
        }

        if (await repository.Remind.ExistsCopyAsync(original.OriginalMessageId, Context.User))
            throw new ValidationException(Texts["RemindModule/Copy/CopyExists", Locale]);

        var fromUser = await Context.Client.FindUserAsync(original.FromUserId.ToUlong());

        CreateRemind.Init(Context);
        await CreateRemind.ProcessAsync(fromUser, Context.User, original.At, original.Message, original.OriginalMessageId!.ToUlong());
    }
}
