using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Commands.Reminder;

public class CreateRemind : CommandAction
{
    private ITextsManager Texts { get; }
    private IConfiguration Configuration { get; }
    private FormatHelper FormatHelper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public CreateRemind(ITextsManager texts, IConfiguration configuration, FormatHelper formatHelper, GrillBotDatabaseBuilder databaseBuilder)
    {
        Texts = texts;
        Configuration = configuration;
        FormatHelper = formatHelper;
        DatabaseBuilder = databaseBuilder;
    }

    private int MinimalTimeMinutes => Configuration.GetValue<int>("Reminder:MinimalTimeMinutes");
    private string MinimalTime => FormatHelper.FormatNumber("RemindModule/Create/Validation/MinimalTime", Locale, MinimalTimeMinutes);

    public async Task<long> ProcessAsync(IUser from, IUser to, DateTime at, string? message, ulong originalMessageId)
    {
        ValidateInput(at, message);

        var entity = new RemindMessage
        {
            At = at,
            Language = Locale,
            Message = message!,
            OriginalMessageId = originalMessageId.ToString(),
            FromUserId = from.Id.ToString(),
            ToUserId = to.Id.ToString()
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.User.GetOrCreateUserAsync(from);
        if (from.Id != to.Id)
            await repository.User.GetOrCreateUserAsync(to);
        await repository.AddAsync(entity);
        await repository.CommitAsync();

        return entity.Id;
    }

    private void ValidateInput(DateTime at, string? message)
    {
        ValidateTime(at);
        ValidateMessage(message);
    }

    private void ValidateTime(DateTime at)
    {
        if (DateTime.Now > at)
            throw new ValidationException(Texts["RemindModule/Create/Validation/MustInFuture", Locale]);

        if ((at - DateTime.Now).TotalMinutes <= MinimalTimeMinutes)
            throw new ValidationException(Texts["RemindModule/Create/Validation/MinimalTimeTemplate", Locale].FormatWith(MinimalTime));
    }

    private void ValidateMessage(string? message)
    {
        message = message?.Trim();
        if (string.IsNullOrEmpty(message))
            throw new ValidationException(Texts["RemindModule/Create/Validation/MessageRequired", Locale]);

        if (message.Length >= EmbedFieldBuilder.MaxFieldValueLength)
            throw new ValidationException(Texts["RemindModule/Create/Validation/MaxLengthExceeded", Locale]);
    }
}
