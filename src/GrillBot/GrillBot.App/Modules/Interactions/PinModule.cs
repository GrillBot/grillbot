using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("pin", "Pins management")]
[ExcludeFromCodeCoverage]
public class PinModule : InteractionsModuleBase
{
    public PinModule(IServiceProvider serviceProvider) : base(null, serviceProvider)
    {
    }

    [SlashCommand("purge", "Unpinn N messages")]
    [SuppressDefer]
    public async Task ProcessAsync(int count, ITextChannel channel = null)
    {
        await DeferAsync(true);
        using var command = GetCommand<Actions.Commands.PurgePins>();

        var result = await command.Command.ProcessAsync(count, channel);
        await SetResponseAsync(result, secret: true);
    }
}
