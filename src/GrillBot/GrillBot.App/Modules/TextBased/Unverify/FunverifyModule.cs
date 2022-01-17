using Discord;
using Discord.Commands;
using GrillBot.Data.Services.Unverify;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RequireUserPermsAttribute = GrillBot.Data.Infrastructure.Preconditions.RequireUserPermissionAttribute;

namespace GrillBot.Data.Modules.TextBased.Unverify;

[Name("Falešné odebrání přístupu")]
[RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
public class FunverifyModule : Infrastructure.ModuleBase
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
    [RequireUserPerms(new[] { GuildPermission.ManageRoles }, false)]
    public async Task FunverifyAsync([Name("datum konce")] DateTime end, [Remainder][Name("duvod a tagy")] string data)
    {
        bool success = true;

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
