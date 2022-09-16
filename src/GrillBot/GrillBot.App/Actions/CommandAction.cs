using Discord.Interactions;

namespace GrillBot.App.Actions;

public abstract class CommandAction
{
    protected IInteractionContext Context { get; private set; }

    public CommandAction()
    {
    }

    public void Init(IInteractionContext context)
    {
        Context = context;
    }
}
