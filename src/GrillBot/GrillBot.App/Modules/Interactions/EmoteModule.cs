using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.Emotes;

namespace GrillBot.App.Modules.Interactions;

[Group("emote", "Správa emotů")]
[RequireUserPerms]
public class EmoteModule : Infrastructure.InteractionsModuleBase
{
    private EmoteService EmoteService { get; }

    public EmoteModule(EmoteService emoteService)
    {
        EmoteService = emoteService;
    }

    [SlashCommand("get", "Informace o emote")]
    public async Task GetEmoteInfoAsync(
        [Summary("emote", "Identifikace emote (ID/Název/Celý emote)")] IEmote emote
    )
    {
        try
        {
            var result = await EmoteService.GetEmoteInfoAsync(emote, Context.User);
            if (result == null)
                await SetResponseAsync("U tohoto emote ještě nebyla zaznamenána žádná aktivita.");
            else
                await SetResponseAsync(embed: result);
        }
        catch (ArgumentException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
