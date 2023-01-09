using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions;

public abstract class CommandAction
{
    protected IInteractionContext Context { get; private set; } = null!;

    protected string Locale
    {
        get
        {
            var locale = Context.Interaction?.UserLocale ?? "";
            return TextsManager.IsSupportedLocale(locale) ? locale : TextsManager.DefaultLocale;
        }
    }

    public void Init(IInteractionContext context)
    {
        Context = context;
    }
}
