using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System.Security.Claims;

namespace GrillBot.App.Services.Reminder;

[Initializable]
public class RemindService : ServiceBase
{
    private IConfiguration Configuration { get; }
    private AuditLogService AuditLogService { get; }

    public RemindService(DiscordSocketClient client, GrillBotDatabaseBuilder dbFactory,
        IConfiguration configuration, AuditLogService auditLogService) : base(client, dbFactory)
    {
        Configuration = configuration;
        AuditLogService = auditLogService;
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

        await using var repository = DbFactory.CreateRepository();

        var fromUser = await repository.User.GetOrCreateUserAsync(from);
        var toUser = from != to ? await repository.User.GetOrCreateUserAsync(to) : fromUser;

        var remind = new RemindMessage()
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
        using var context = DbFactory.Create();

        return await context.Users
            .Include(o => o.IncomingReminders)
            .Where(o => o.Id == forUser.Id.ToString())
            .SelectMany(o => o.IncomingReminders)
            .Where(o => o.RemindMessageId == null)
            .CountAsync();
    }

    public async Task<List<RemindMessage>> GetRemindersAsync(IUser forUser, int page)
    {
        using var context = DbFactory.Create();

        var remindersQuery = context.Users
            .Include(o => o.IncomingReminders)
            .Where(o => o.Id == forUser.Id.ToString())
            .SelectMany(o => o.IncomingReminders)
            .Include(o => o.FromUser)
            .Where(o => o.RemindMessageId == null)
            .OrderBy(o => o.At)
            .ThenBy(o => o.Id)
            .Skip(page * EmbedBuilder.MaxFieldCount)
            .Take(EmbedBuilder.MaxFieldCount);

        return await remindersQuery.ToListAsync();
    }

    public async Task CopyAsync(long originalRemindId, IUser toUser)
    {
        using var context = DbFactory.Create();

        var original = await context.Reminders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == originalRemindId);

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

        var exists = await context.Reminders.AnyAsync(o => o.OriginalMessageId == original.OriginalMessageId && o.ToUserId == toUser.Id.ToString());
        if (exists)
            throw new ValidationException("Toto upozornění jsi již jednou z tlačítka vytvořil. Nemůžeš vytvořit další.");

        var fromUser = await DiscordClient.FindUserAsync(original.FromUserId.ToUlong());
        if (fromUser == null)
            throw new ValidationException("Uživatel, který založil toto upozornění se nepodařilo dohledat");

        await CreateRemindAsync(fromUser, toUser, original.At, original.Message, original.OriginalMessageId.ToUlong());
    }

    /// <summary>
    /// Cancels remind.
    /// </summary>
    /// <param name="id">ID of remind</param>
    /// <param name="user">Destination user</param>
    /// <param name="notify">Send notifiaction before cancel.</param>
    public async Task CancelRemindAsync(long id, IUser user, bool notify = false)
    {
        using var context = DbFactory.Create();

        var remind = await context.Reminders.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

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
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Service cancellation of remind.
    /// </summary>
    public async Task ServiceCancellationAsync(long id, ClaimsPrincipal loggedUser, bool notify = false)
    {
        using var context = DbFactory.Create();

        var remind = await context.Reminders.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (remind == null) throw new NotFoundException("Požadované upozornění neexistuje.");
        if (!string.IsNullOrEmpty(remind.RemindMessageId))
        {
            if (remind.RemindMessageId == "0")
                throw new InvalidOperationException("Nelze zrušit již zrušené upozornění.");

            throw new InvalidOperationException("Nelze zrušit již proběhlé upozornění.");
        }

        ulong messageId = 0;
        if (notify)
            messageId = await SendNotificationMessageAsync(remind, true);

        var logItem = new AuditLogDataWrapper(
            AuditLogItemType.Info,
            $"Bylo stornováno upozornění s ID {id}. {(notify ? "Při rušení bylo odesláno upozornění uživateli." : "")}".Trim(),
            null, null, await DiscordClient.FindUserAsync(loggedUser.GetUserId())
        );
        await AuditLogService.StoreItemAsync(logItem);

        remind.RemindMessageId = messageId.ToString();
        await context.SaveChangesAsync();
    }

    private async Task<ulong> SendNotificationMessageAsync(RemindMessage remind, bool force = false)
    {
        var embed = (await CreateRemindEmbedAsync(remind, force))?.Build();

        if (embed != null)
        {
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
        }

        return 0;
    }

    private async Task<EmbedBuilder> CreateRemindEmbedAsync(RemindMessage remind, bool force = false)
    {
        var embed = new EmbedBuilder()
            .WithAuthor(DiscordClient.CurrentUser)
            .WithColor(force ? Color.Gold : Color.Green)
            .WithCurrentTimestamp();

        if (force)
            embed.WithTitle("Máš předčasně nové upozornění.");
        else
            embed.WithTitle("Máš nové upozornění.");

        embed
            .AddField("ID", remind.Id, true);

        if (remind.FromUserId != remind.ToUserId)
        {
            var fromUser = await DiscordClient.FindUserAsync(remind.FromUserId.ToUlong());

            if (fromUser != null)
                embed.AddField("Od", fromUser.GetFullName(), true);
        }

        if (remind.Postpone > 0)
            embed.AddField("POZOR", $"Toto upozornění bylo odloženo už {remind.Postpone}x", false);

        embed
            .AddField("Zpráva", remind.Message, false);

        if (!force)
            embed.AddField("Možnosti", "Pokud si přeješ toto upozornění posunout, tak klikni na příslušnou reakci podle počtu hodin.");

        return embed;
    }

    public async Task<List<long>> GetRemindIdsForProcessAsync()
    {
        await using var repository = DbFactory.CreateRepository();
        return await repository.Remind.GetRemindIdsForProcessAsync();
    }

    public async Task ProcessRemindFromJobAsync(long id)
    {
        await using var repository = DbFactory.CreateRepository();

        var remind = await repository.Remind.FindRemindByIdAsync(id);
        if (remind == null) return;

        var embed = (await CreateRemindEmbedAsync(remind, false)).Build();
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
        using var context = DbFactory.Create();

        var query = context.Users
            .Where(o => o.Id == user.Id.ToString())
            .Select(o => new
            {
                Incoming = o.IncomingReminders
                    .Where(o => o.RemindMessageId == null)
                    .Select(x => new { x.Id, x.At, x.FromUser }),
                Outgoing = o.OutgoingReminders
                    .Where(o => o.RemindMessageId == null)
                    .Select(x => new { x.Id, x.At, x.ToUser })
            });

        var data = await query.FirstOrDefaultAsync();

        var incoming = data.Incoming.ToDictionary(o => o.Id, o => $"Příchozí #{o.Id} ({o.At.ToCzechFormat()}) od uživatele {o.FromUser.Username}#{o.FromUser.Discriminator}");
        var outgoing = data.Outgoing.ToDictionary(o => o.Id, o => $"Odchozí #{o.Id} ({o.At.ToCzechFormat()}) pro uživatele {o.ToUser.Username}#{o.ToUser.Discriminator}");

        return incoming
            .Concat(outgoing)
            .DistinctBy(o => o.Key)
            .OrderBy(o => o.Key)
            .Take(25)
            .ToDictionary(o => o.Key, o => o.Value);
    }
}
