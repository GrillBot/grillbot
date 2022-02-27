#pragma warning disable IDE0060 // Remove unused parameter
using Discord.Commands;
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
    [TextCommandDeprecated(AlternativeCommand = "/selfunverify", AdditionalMessage = "Administrace pro ponechatelné přístupy byla přesunuta do webové administrace.")]
    public Task SelfunverifyAsync([Name("datum konce")] DateTime end, [Name("seznam ponechatelnych")] params string[] keeps) => Task.CompletedTask;

    [Group("keep")]
    [Name("Ponechatelné přístupy pro selfunverify")]
    [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidávat reakce.")]
    [RequireUserPerms(GuildPermission.ManageRoles)]
    [TextCommandDeprecated(AlternativeCommand = "/bot selfunverify list-keepables", AdditionalMessage = "Administrační metody byly přesunuty do webové administrace.")]
    public class SelfunverifyKeepableSubModule : Infrastructure.ModuleBase
    {
        [Command("add")]
        public Task AddAsync([Name("skupina")] string _, [Name("nazev")] string __) => Task.CompletedTask;

        [Command("remove")]
        public Task RemoveAsync([Name("skupina")] string _, [Name("nazev")] string __ = null) => Task.CompletedTask;

        [Command("list")]
        public Task ListAsync([Name("skupina")] string _ = null) => Task.CompletedTask;
    }
}
