using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Infrastructure.Common;

[ExcludeFromCodeCoverage]
public abstract class CommandActionTest<TAction> : ActionTest<TAction> where TAction : CommandAction
{
    protected virtual IGuild Guild { get; }
    protected virtual IGuildUser User { get; }
    protected virtual IDiscordInteraction Interaction { get; }
    protected virtual IMessageChannel Channel { get; }
    protected virtual IDiscordClient Client { get; }
    protected IInteractionContext Context { get; private set; }

    protected override bool CanInitProvider => false;

    protected override void Init()
    {
        Context = new InteractionContextBuilder().SetGuild(Guild).SetUser(User).SetInteraction(Interaction).SetChannel(Channel).SetClient(Client).Build();
    }

    protected TAction InitAction(TAction action)
    {
        action.Init(Context);
        return action;
    }
}
