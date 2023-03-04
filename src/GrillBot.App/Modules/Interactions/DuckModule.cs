using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
public class DuckModule : InteractionsModuleBase
{
    public DuckModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("duck", "Finds the current state of the duck club.")]
    public async Task GetDuckInfoAsync()
    {
        using var command = GetCommand<Actions.Commands.DuckInfo>();

        try
        {
            var result = await command.Command.ProcessAsync();
            await SetResponseAsync(embed: result);
        }
        catch (GrillBotException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
