using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions;

public abstract class CommandAction
{
    protected IInteractionContext Context { get; private set; }

    protected string Locale
    {
        get
        {
            var locale = Context?.Interaction?.UserLocale ?? "";
            return TextsManager.IsSupportedLocale(locale) ? locale : TextsManager.DefaultLocale;
        }
    }

    public CommandAction()
    {
    }

    public void Init(IInteractionContext context)
    {
        Context = context;
    }
}
