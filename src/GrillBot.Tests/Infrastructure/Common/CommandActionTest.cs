using Discord;
using GrillBot.App.Actions;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Infrastructure.Common;

public abstract class CommandActionTest<TAction> : TestBase<TAction> where TAction : CommandAction
{
    protected virtual IGuild? Guild { get; } = null;
    protected virtual IGuildUser? User { get; } = null;
    protected virtual IDiscordInteraction? Interaction { get; } = null;
    protected virtual IMessageChannel? Channel { get; } = null;
    protected virtual IDiscordClient? Client { get; } = null;
    protected IInteractionContext Context { get; private set; } = null!;

    protected override void PreInit()
    {
        var builder = new InteractionContextBuilder();

        if (Guild != null) builder = builder.SetGuild(Guild);
        if (User != null) builder = builder.SetUser(User);
        if (Interaction != null) builder = builder.SetInteraction(Interaction);
        if (Channel != null) builder = builder.SetChannel(Channel);
        if (Client != null) builder = builder.SetClient(Client);

        Context = builder.Build();
    }

    protected TAction InitAction(TAction action)
    {
        action.Init(Context);
        return action;
    }
}
