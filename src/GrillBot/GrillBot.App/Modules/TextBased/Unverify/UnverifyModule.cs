using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.Unverify;
using GrillBot.Common;
using GrillBot.Data.Exceptions;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased.Unverify;

[Group("unverify")]
[Name("Odebrání přístupu")]
[Infrastructure.Preconditions.TextBased.RequireUserPerms(GuildPermission.ManageRoles)]
public class UnverifyModule : ModuleBase
{
    private UnverifyService UnverifyService { get; }
    private IConfiguration Configuration { get; }

    public UnverifyModule(UnverifyService unverifyService, IConfiguration configuration)
    {
        UnverifyService = unverifyService;
        Configuration = configuration;
    }

    [Command("")]
    [Summary("Dočasné odebrání přístupu na serveru.\n" +
             "Datum konce se dá zapsat přímo jako datum, nebo jako časový posun. Např.: `30m`, nebo `2021-07-02T15:30:25`. Koncovky časového posunu jsou: **m**inuty, **h**odiny, **d**ny, **M**ěsíce, **r**oky.\n" +
             "Dále je důvod, proč daná osoba přišla o přístup. Nakonec se zadávají uživatelé **formou tagů**.\n" +
             "Celý příkaz pak vypadá např.: `{prefix}unverify 2h Odebral jsem ti přístup. @GrillBot`"
    )]
    [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
    [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění kanálů a role.")]
    public async Task SetUnverifyAsync([Name("datum konce")] DateTime end, [Remainder] [Name("duvod a tagy")] string data)
    {
        var success = true;

        try
        {
            await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

            var users = Context.Message.MentionedUsers.Where(o => o != null).ToList();
            var mentionedRoles = Context.Message.MentionedRoles.Where(o => !o.IsEveryone).ToList();
            if (mentionedRoles.Count > 0) users.AddRange(mentionedRoles.SelectMany(o => o.Members));
            users = users.GroupBy(o => o.Id).Select(o => o.First()).ToList();
            if (users.Count == 0) return;

            var messages = await UnverifyService.SetUnverifyAsync(users, end, data, Context.Guild, Context.User, false, "cs");
            foreach (var message in messages)
            {
                await ReplyAsync(message);
            }
        }
        catch (Exception ex)
        {
            success = false;

            if (ex is ValidationException)
                await ReplyAsync(ex.Message);
            else
                throw;
        }
        finally
        {
            await Context.Message.RemoveAllReactionsAsync();

            if (success)
                await Context.Message.AddReactionAsync(Emojis.Ok);
        }
    }

    [Command("remove")]
    [Summary("Předčasné vrácení přístupu.\n" +
             "Zadává se identifikace uživatele. To znamená ID uživatele, tag, nebo jméno\n" +
             "Celý příkaz pak vypadá např.: {prefix}unverify remove @GrillBot")]
    [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
    [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění kanálů a role.")]
    public async Task RemoveUnverifyAsync([Name("kdo")] IGuildUser user)
    {
        var success = true;

        try
        {
            await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

            var fromUser = Context.User as IGuildUser ?? Context.Guild.GetUser(Context.User.Id);
            var message = await UnverifyService.RemoveUnverifyAsync(Context.Guild, fromUser, user, "cs");
            await ReplyAsync(message);
        }
        catch (Exception)
        {
            success = false;
            throw;
        }
        finally
        {
            await Context.Message.RemoveAllReactionsAsync();

            if (success)
                await Context.Message.AddReactionAsync(Emojis.Ok);
        }
    }

    [Command("update")]
    [Summary("Aktualizace času u záznamu o dočasném odebrání přístupu.\n" +
             "Formát data o novém konci unverify je stejný jako při zadávání unverify.\n" +
             "Identifikace uživatele je stejná jako u příkazu `{prefix}unverify remove`." +
             "Celý příkaz vypadá např.: `{prefix}unverify update @GrillBot 1h`")]
    public async Task UnverifyUpdateAsync([Name("kdo")] IGuildUser user, [Name("novy datum konce")] DateTime end)
    {
        try
        {
            var fromUser = Context.User as IGuildUser ?? Context.Guild.GetUser(Context.User.Id);
            var message = await UnverifyService.UpdateUnverifyAsync(user, Context.Guild, end, fromUser, "cs");
            await ReplyAsync(message);
        }
        catch (Exception ex)
        {
            if (ex is not ValidationException && ex is not NotFoundException)
                throw;

            await ReplyAsync(ex.Message);
        }
    }

    [Command("list")]
    [TextCommandDeprecated(AlternativeCommand = "/unverify list")]
    public Task UnverifyListAsync() => Task.CompletedTask;
}
