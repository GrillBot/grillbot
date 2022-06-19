using Discord.Commands;
using GrillBot.App.Services.Unverify;
using GrillBot.Common;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;
using RequireUserPerms = GrillBot.App.Infrastructure.Preconditions.TextBased.RequireUserPermsAttribute;

namespace GrillBot.App.Modules.TextBased.Unverify;

[Name("Falešné odebrání přístupu")]
[RequireUserPerms(GuildPermission.ManageRoles)]
public class FunverifyModule : ModuleBase
{
    private UnverifyService UnverifyService { get; }
    private IConfiguration Configuration { get; }

    public FunverifyModule(UnverifyService unverifyService, IConfiguration configuration)
    {
        UnverifyService = unverifyService;
        Configuration = configuration;
    }

    [Command("funverify")]
    [Summary("Falešné odebrání přístupu uživateli.")]
    [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
    [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění kanálů a role.")]
    public async Task FunverifyAsync([Name("datum konce")] DateTime end, [Remainder][Name("duvod a tagy")] string data)
    {
        var success = true;

        try
        {
            await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

            var users = Context.Message.MentionedUsers.Where(o => o != null).ToList();
            if (users.Count == 0) return;

            var messages = await UnverifyService.SetUnverifyAsync(users, end, data, Context.Guild, Context.User, true);
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

        await Task.Delay(Configuration.GetValue<int>("Unverify:FunverifySleepTime"));
        await Context.Channel.SendMessageAsync(Configuration["Discord:Emotes:KappaLul"]);
    }
}
