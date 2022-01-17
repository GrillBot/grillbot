using Discord.Commands;

namespace GrillBot.Data.Infrastructure.Commands
{
    public class CommandRedirectResult : RuntimeResult
    {
        public string NewCommand { get; set; }

        public CommandRedirectResult(string newCommand) : base(CommandError.Unsuccessful, null)
        {
            NewCommand = newCommand;
        }
    }
}
