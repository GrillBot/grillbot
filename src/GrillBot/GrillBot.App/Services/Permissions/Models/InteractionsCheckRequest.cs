using Discord.Interactions;

namespace GrillBot.App.Services.Permissions.Models
{
    public class InteractionsCheckRequest : CheckRequestBase
    {
        public IInteractionContext InteractionContext { get; set; }
        public ICommandInfo CommandInfo { get; set; }

        public override IUser User => InteractionContext.User;
        public override IGuild Guild => InteractionContext.Guild;
        public override IMessageChannel Channel => InteractionContext.Channel;
        public override IDiscordClient DiscordClient => InteractionContext.Client;
        public override string CommandName => CommandInfo.Name;
    }
}