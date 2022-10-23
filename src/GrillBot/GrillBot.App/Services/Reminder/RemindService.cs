using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Reminder;

[Initializable]
public class RemindService
{
    private IConfiguration Configuration { get; }
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public RemindService(IDiscordClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, ITextsManager texts)
    {
        Configuration = configuration;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task<long> CreateRemindAsync(IUser from, IUser to, DateTime at, string message, ulong originalMessageId, string locale)
    {
        if (DateTime.Now > at)
            throw new ValidationException("Datum a čas upozornění musí být v budoucnosti.");

        var minimalTime = Configuration.GetValue<int>("Reminder:MinimalTimeMinutes");
        if ((at - DateTime.Now).TotalMinutes <= minimalTime)
        {
            var suffix = "minuta";
            switch (minimalTime)
            {
                case 0:
                case >= 5:
                    suffix = "minut";
                    break;
                case > 1 and < 5:
                    suffix = "minuty";
                    break;
            }

            throw new ValidationException($"Datum a čas upozornění musí být později, než {minimalTime} {suffix}");
        }

        if (string.IsNullOrEmpty(message))
            throw new ValidationException("Text upozornění je povinný.");

        message = message.Trim();
        if (message.Length >= EmbedFieldBuilder.MaxFieldValueLength)
            throw new ValidationException($"Maximální délka zprávy může být {EmbedFieldBuilder.MaxFieldValueLength} znaků.");

        await using var repository = DatabaseBuilder.CreateRepository();

        var fromUser = await repository.User.GetOrCreateUserAsync(from);
        var toUser = from.Id != to.Id ? await repository.User.GetOrCreateUserAsync(to) : fromUser;

        var remind = new RemindMessage
        {
            At = at,
            FromUser = fromUser,
            Message = message,
            OriginalMessageId = originalMessageId.ToString(),
            ToUser = toUser,
            Language = TextsManager.FixLocale(locale)
        };

        await repository.AddAsync(remind);
        await repository.CommitAsync();

        return remind.Id;
    }

    public async Task<int> GetRemindersCountAsync(IUser forUser)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindersCountAsync(forUser);
    }

    public async Task<List<RemindMessage>> GetRemindersAsync(IUser forUser, int page)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindersPageAsync(forUser, page);
    }

    public async Task CopyAsync(long originalRemindId, IUser toUser, string locale)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var original = await repository.Remind.FindRemindByIdAsync(originalRemindId);

        if (original == null)
            throw new InvalidOperationException("Upozornění nebylo nalezeno.");

        // User cannot copy reminders that created for himself.
        if (original.FromUserId == toUser.Id.ToString() && original.ToUserId == toUser.Id.ToString())
            throw new ValidationException("Toto upozornění jsi založil, nemůžeš dostat to stejné.");

        if (!string.IsNullOrEmpty(original.RemindMessageId))
        {
            if (original.RemindMessageId == "0")
                throw new InvalidOperationException("Toto upozornění bylo zrušeno.");

            throw new InvalidOperationException("Toto upozornění již bylo odesláno.");
        }

        if (await repository.Remind.ExistsCopyAsync(original.OriginalMessageId, toUser))
            throw new ValidationException("Toto upozornění jsi již jednou z tlačítka vytvořil. Nemůžeš vytvořit další.");

        var fromUser = await DiscordClient.FindUserAsync(original.FromUserId.ToUlong());
        if (fromUser == null)
            throw new ValidationException("Uživatel, který založil toto upozornění se nepodařilo dohledat");

        await CreateRemindAsync(fromUser, toUser, original.At, original.Message, original.OriginalMessageId!.ToUlong(), locale);
    }

    public async Task<Dictionary<long, string>> GetRemindSuggestionsAsync(IUser user, string locale)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Remind.GetRemindSuggestionsAsync(user);
        var userId = user.Id.ToString();

        var incoming = data
            .Where(o => o.ToUserId == userId)
            .ToDictionary(o => o.Id, o => Texts["RemindModule/Suggestions/Incoming", locale].FormatWith(o.Id, o.At.ToCzechFormat(), o.FromUser!.FullName()));

        var outgoing = data
            .Where(o => o.FromUserId == userId)
            .ToDictionary(o => o.Id, o => Texts["RemindModule/Suggestions/Outgoing", locale].FormatWith(o.Id, o.At.ToCzechFormat(), o.ToUser!.FullName()));

        return incoming
            .Concat(outgoing)
            .DistinctBy(o => o.Key)
            .OrderBy(o => o.Key)
            .Take(25)
            .ToDictionary(o => o.Key, o => o.Value);
    }
}
