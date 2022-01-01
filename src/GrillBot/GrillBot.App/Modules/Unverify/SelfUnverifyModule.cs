using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services.Unverify;
using GrillBot.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RequireUserPermsAttribute = GrillBot.App.Infrastructure.Preconditions.RequireUserPermissionAttribute;

namespace GrillBot.App.Modules.Unverify
{
    [Group("selfunverify")]
    [Name("Selfunverify")]
    [Summary("Odebrání přístupu sebe sama.")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
    public class SelfUnverifyModule : Infrastructure.ModuleBase
    {
        private SelfunverifyService SelfunverifyService { get; }
        private IConfiguration Configuration { get; }

        public SelfUnverifyModule(SelfunverifyService service, IConfiguration configuration)
        {
            SelfunverifyService = service;
            Configuration = configuration;
        }

        [Command("")]
        [Summary("Dočasné odebrání přístupu sobě sama na serveru.\n" +
            "Datum konce se dá zapsat přímo jako datum, nebo jako časový posun. Např.: `30m`, nebo `2021-07-02T15:30:25`. Koncovky časového posunu jsou: **m**inuty, **h**odiny, **d**ny, **M**ěsíce, **r**oky.\n" +
            "Dále je seznam rolí a kanálů, které si přeje uživatel ponechat. Maximálně si jich lze ponechat 10. Seznam se zadává slovně. **Pouze názvy.**\n" +
            "Celý příkaz pak vypadá např.: `{prefix}selfunverify 2h ROLE`"
        )]
        [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění kanálů a role.")]
        public async Task SelfunverifyAsync([Name("datum konce")] DateTime end, [Name("seznam ponechatelnych")] params string[] keeps)
        {
            bool success = true;

            try
            {
                await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                end = end.AddMinutes(1); // Because checks are strict.
                var tokeep = keeps.Distinct().Select(o => o.ToLower()).ToList();
                var message = await SelfunverifyService.ProcessSelfUnverifyAsync(Context.User, end, Context.Guild, tokeep);
                await ReplyAsync(message);
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

        [Group("keep")]
        [Name("Ponechatelné přístupy pro selfunverify")]
        [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
        [RequireUserPerms(new[] { GuildPermission.ManageRoles }, false)]
        public class SelfunverifyKeepableSubModule : Infrastructure.ModuleBase
        {
            private SelfunverifyService SelfunverifyService { get; }

            public SelfunverifyKeepableSubModule(SelfunverifyService service)
            {
                SelfunverifyService = service;
            }

            [Command("add")]
            [Summary("Přidá ponechatelný přístup.")]
            public async Task AddAsync([Name("skupina")] string group, [Name("nazev")] string name)
            {
                try
                {
                    await SelfunverifyService.AddKeepableAsync(group, name);
                    await Context.Message.AddReactionAsync(Emojis.Ok);
                }
                catch (ValidationException ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }

            [Command("remove")]
            [Summary("Odebere ponechatelný přístup.")]
            public async Task RemoveAsync([Name("skupina")] string group, [Name("nazev")] string name = null)
            {
                try
                {
                    await SelfunverifyService.RemoveKeepableAsync(group, name);
                    await Context.Message.AddReactionAsync(Emojis.Ok);
                }
                catch (ValidationException ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }

            [Command("list")]
            [Summary("Vypíše seznam ponechatelných přístupů. S možností zobrazit určitou skupinu.")]
            public async Task ListAsync([Name("skupina")] string group = null)
            {
                var data = await SelfunverifyService.GetKeepablesAsync(group);

                if (data.Count == 0)
                {
                    await ReplyAsync("Nebyly nalezeny žádné ponechatelné přístupy.");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .WithFooter(Context.User)
                    .WithTitle("Ponechatelné role a kanály");

                foreach (var grp in data.GroupBy(o => string.Join("|", o.Value)))
                {
                    var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? "Ostatní" : o.Key));

                    foreach (var part in grp.First().Value.SplitInParts(50))
                        embed.AddField(keys, string.Join(", ", part), false);
                }

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}
