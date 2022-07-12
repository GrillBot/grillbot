using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Reminder;

[Initializable]
public class RemindService
{
    private IConfiguration Configuration { get; }
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemindService(IDiscordClient client, GrillBotDatabaseBuilder databaseBuilder,
        IConfiguration configuration)
    {
        Configuration = configuration;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<long> CreateRemindAsync(IUser from, IUser to, DateTime at, string message, ulong originalMessageId)
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
            ToUser = toUser
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

    public async Task CopyAsync(long originalRemindId, IUser toUser)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var original = await repository.Remind.FindRemindByIdAsync(originalRemindId);

        if (original == null)
            throw new InvalidOperationException("Upozornění nebylo nalezeno.");

        if (original.FromUserId == toUser.Id.ToString())
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

        await CreateRemindAsync(fromUser, toUser, original.At, original.Message, original.OriginalMessageId!.ToUlong());
    }

    /// <summary>
    /// Cancels remind.
    /// </summary>
    /// <param name="id">ID of remind</param>
    /// <param name="user">Destination user</param>
    /// <param name="notify">Send notifiaction before cancel.</param>
    public async Task CancelRemindAsync(long id, IUser user, bool notify = false)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var remind = await repository.Remind.FindRemindByIdAsync(id);

        if (remind == null)
            throw new ValidationException("Upozornění nebylo nalezeno.");

        if (!string.IsNullOrEmpty(remind.RemindMessageId))
        {
            if (remind.RemindMessageId == "0")
                throw new ValidationException("Toto upozornění již bylo dříve zrušeno.");

            throw new ValidationException("Toto upozornění již bylo odesláno.");
        }

        if (remind.FromUserId != user.Id.ToString() && remind.ToUserId != user.Id.ToString())
            throw new ValidationException("Upozornění může zrušit pouze ten, komu je určeno, nebo kdo jej založil.");

        ulong messageId = 0;
        if (notify)
            messageId = await SendNotificationMessageAsync(remind, true);

        remind.RemindMessageId = messageId.ToString();
        await repository.CommitAsync();
    }

    public async Task<ulong> SendNotificationMessageAsync(RemindMessage remind, bool force = false)
    {
        var embed = (await CreateRemindEmbedAsync(remind, force))?.Build();

        if (embed == null)
            return 0;

        var toUser = await DiscordClient.FindUserAsync(remind.ToUserId.ToUlong());
        try
        {
            if (toUser != null)
                return (await toUser.SendMessageAsync(embed: embed)).Id;
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have disabled DMs.
            return 0;
        }

        return 0;
    }

    private async Task<EmbedBuilder> CreateRemindEmbedAsync(RemindMessage remind, bool force = false)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(DiscordClient.CurrentUser)
            .WithColor(force ? Color.Gold : Color.Green)
            .WithCurrentTimestamp()
            .WithTitle(force ? "Máš předčasně nové upozornění." : "Máš nové upozornění.")
            .AddField("ID", remind.Id, true);

        if (remind.FromUserId != remind.ToUserId)
        {
            var fromUser = await DiscordClient.FindUserAsync(remind.FromUserId.ToUlong());

            if (fromUser != null)
                embed.AddField("Od", fromUser.GetFullName(), true);
        }

        if (remind.Postpone > 0)
            embed.AddField("POZOR", $"Toto upozornění bylo odloženo už {remind.Postpone}x");

        embed
            .AddField("Zpráva", remind.Message);

        if (!force)
            embed.AddField("Možnosti", "Pokud si přeješ toto upozornění posunout, tak klikni na příslušnou reakci podle počtu hodin.");

        return embed;
    }

    public async Task<List<long>> GetRemindIdsForProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindIdsForProcessAsync();
    }

    public async Task ProcessRemindFromJobAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var remind = await repository.Remind.FindRemindByIdAsync(id);
        if (remind == null) return;

        var embed = (await CreateRemindEmbedAsync(remind)).Build();
        var toUser = await DiscordClient.FindUserAsync(remind.ToUserId.ToUlong());

        ulong messageId = 0;
        try
        {
            if (toUser != null)
            {
                var msg = await toUser.SendMessageAsync(embed: embed);
                var hourEmojis = Emojis.NumberToEmojiMap.Where(o => o.Key > 0).Select(o => o.Value);
                await msg.AddReactionsAsync(hourEmojis.ToArray());

                messageId = msg.Id;
            }
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            // User have disabled DMs.
        }

        remind.RemindMessageId = messageId.ToString();
        await repository.CommitAsync();
    }

    public async Task<Dictionary<long, string>> GetRemindSuggestionsAsync(IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Remind.GetRemindSuggestionsAsync(user);
        var userId = user.Id.ToString();

        var incoming = data
            .Where(o => o.ToUserId == userId)
            .ToDictionary(o => o.Id, o => $"Příchozí #{o.Id} ({o.At.ToCzechFormat()}) od uživatele {o.FromUser!.Username}#{o.FromUser!.Discriminator}");

        var outgoing = data
            .Where(o => o.FromUserId == userId)
            .ToDictionary(o => o.Id, o => $"Odchozí #{o.Id} ({o.At.ToCzechFormat()}) pro uživatele {o.ToUser!.Username}#{o.ToUser!.Discriminator}");

        return incoming
            .Concat(outgoing)
            .DistinctBy(o => o.Key)
            .OrderBy(o => o.Key)
            .Take(25)
            .ToDictionary(o => o.Key, o => o.Value);
    }
}
