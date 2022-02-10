using Discord.Commands;
using GrillBot.App.Services.Permissions.Models;

namespace GrillBot.App.Services.Permissions
{
    public class CommandsCheckRequest : CheckRequestBase
    {
        public ContextType? Context { get; set; }

        public ICommandContext CommandContext { get; set; }
        public CommandInfo CommandInfo { get; set; }

        public override IUser User => CommandContext.User;
        public override IGuild Guild => CommandContext.Guild;
        public override IMessageChannel Channel => CommandContext.Channel;
        public override IDiscordClient DiscordClient => CommandContext.Client;
        public override string CommandName => CommandInfo.Aliases[0];
    }
}
