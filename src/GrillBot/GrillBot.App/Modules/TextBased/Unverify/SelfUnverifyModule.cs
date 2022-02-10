#pragma warning disable IDE0060 // Remove unused parameter
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.Unverify;
using RequireUserPerms = GrillBot.App.Infrastructure.Preconditions.TextBased.RequireUserPermsAttribute;

namespace GrillBot.App.Modules.TextBased.Unverify;

[Group("selfunverify")]
[Name("Selfunverify")]
[Summary("Odebrání přístupu sebe sama.")]
[RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
public class SelfUnverifyModule : Infrastructure.ModuleBase
{
    [Command("")]
    [TextCommandDeprecated(AlternativeCommand = "/selfunverify")]
    public Task SelfunverifyAsync([Name("datum konce")] DateTime end, [Name("seznam ponechatelnych")] params string[] keeps) => Task.CompletedTask;

    [Group("keep")]
    [Name("Ponechatelné přístupy pro selfunverify")]
    [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
    [RequireUserPerms(GuildPermission.ManageRoles)]
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
