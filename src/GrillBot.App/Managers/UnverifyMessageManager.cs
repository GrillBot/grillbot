using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Managers;

public class UnverifyMessageManager
{
    private ITextsManager Texts { get; }

    public UnverifyMessageManager(ITextsManager texts)
    {
        Texts = texts;
    }

    public string CreateRemoveAccessManuallyFailed(IGuildUser user, Exception ex, string locale)
    {
        var username = user.GetFullName();

        return string.Format(Texts["Unverify/Message/ManuallyRemoveFailed", locale], username, ex.Message);
    }

    public string CreateRemoveAccessUnverifyNotFound(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return string.Format(Texts["Unverify/Message/RemoveAccessUnverifyNotFound", locale], username);
    }
}
