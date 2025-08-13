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

        return Texts["Unverify/Message/ManuallyRemoveFailed", locale].FormatWith(username, ex.Message);
    }

    public string CreateRemoveAccessUnverifyNotFound(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return Texts["Unverify/Message/RemoveAccessUnverifyNotFound", locale].FormatWith(username);
    }
}
