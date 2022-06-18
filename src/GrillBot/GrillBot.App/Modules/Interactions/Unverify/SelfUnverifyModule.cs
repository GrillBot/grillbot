using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common;

namespace GrillBot.App.Modules.Interactions.Unverify;

[RequireUserPerms]
public class SelfUnverifyModule : Infrastructure.InteractionsModuleBase
{
    private SelfunverifyService SelfunverifyService { get; }
    private IConfiguration Configuration { get; }

    public SelfUnverifyModule(SelfunverifyService selfunverifyService, IConfiguration configuration)
    {
        SelfunverifyService = selfunverifyService;
        Configuration = configuration;
    }

    [SlashCommand("selfunverify", "Dočasné odebrání přístupu sobě sama na serveru.")]
    [RequireBotPermission(GuildPermission.AddReactions)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task SelfUnverifyAsync(
        [Summary("konec", "Datum a čas konce, nebo doba trvání odebrání přístupu.")]
        DateTime end,
        [Summary("pristupy", "Seznam ponechatelných přístupů. Oddělujte čárkou, mezerou nebo středníkem.")]
        string keepables = null
    )
    {
        var originalMessage = await Context.Interaction.GetOriginalResponseAsync();
        bool success = true;
        keepables ??= "";

        try
        {
            await originalMessage.AddReactionAsync(Emote.Parse(Configuration.GetValue<string>("Discord:Emotes:Loading")));

            end = end.AddMinutes(1); // Strinct checks are only in unverify.
            var toKeep = keepables?.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(o => o.ToLower()).ToList();
            var result = await SelfunverifyService.ProcessSelfUnverifyAsync(Context.User, end, Context.Guild, toKeep);
            await SetResponseAsync(result);
        }
        catch (Exception ex)
        {
            success = false;

            if (ex is ValidationException)
            {
                await SetResponseAsync(ex.Message);
            }
            else
            {
                await SetResponseAsync("Provedení selfunverify se nezdařilo.");
                throw;
            }
        }
        finally
        {
            if (originalMessage != null)
            {
                await originalMessage.RemoveAllReactionsAsync();

                if (success)
                    await originalMessage.AddReactionAsync(Emojis.Ok);
            }
        }
    }
}
