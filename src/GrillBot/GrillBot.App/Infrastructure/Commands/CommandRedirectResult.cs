using Discord.Commands;

namespace GrillBot.App.Infrastructure.Commands;

public class CommandRedirectResult : RuntimeResult
{
    public string NewCommand { get; }

    public CommandRedirectResult(string newCommand) : base(CommandError.Unsuccessful, null)
    {
        NewCommand = newCommand;
    }
}
