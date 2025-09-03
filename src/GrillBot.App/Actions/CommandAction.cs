using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions;

public abstract class CommandAction
{
    private IGuildUser? _currentUser;
    private IGuildUser? _executingUser;

    protected IInteractionContext Context { get; private set; } = null!;
    protected CancellationToken CancellationToken { get; private set; }

    protected string Locale
    {
        get
        {
            var locale = Context.Interaction?.UserLocale ?? "";
            return TextsManager.IsSupportedLocale(locale) ? locale : TextsManager.DefaultLocale;
        }
    }

    public void Init(IInteractionContext context, CancellationToken cancellationToken = default)
    {
        Context = context;
        CancellationToken = cancellationToken;
    }

    protected async Task<IGuildUser> GetExecutingUserAsync()
    {
        _executingUser ??= Context.User as IGuildUser ?? await Context.Guild.GetUserAsync(Context.User.Id);
        return _executingUser;
    }

    protected async Task<IGuildUser> GetCurrentUserAsync()
    {
        _currentUser ??= await Context.Guild.GetCurrentUserAsync();
        return _currentUser;
    }
}
