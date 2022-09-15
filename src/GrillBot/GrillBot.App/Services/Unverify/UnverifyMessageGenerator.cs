using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.Unverify;

namespace GrillBot.App.Services.Unverify;

public class UnverifyMessageGenerator
{
    private LocalizationManager Localization { get; }

    public UnverifyMessageGenerator(LocalizationManager localization)
    {
        Localization = localization;
    }

    public string CreateUnverifyMessageToChannel(UnverifyUserProfile profile, string locale)
    {
        var endDateTime = profile.End.ToCzechFormat();
        var username = profile.Destination.GetDisplayName();

        return profile.IsSelfUnverify
            ? Localization["Unverify/Message/UnverifyToChannelWithoutReason", locale].FormatWith(username, endDateTime)
            : Localization["Unverify/Message/UnverifyToChannelWithReason", locale].FormatWith(username, endDateTime, profile.Reason);
    }

    public string CreateUnverifyPmMessage(UnverifyUserProfile profile, IGuild guild, string locale)
    {
        var endDateTime = profile.End.ToCzechFormat();

        return profile.IsSelfUnverify
            ? Localization["Unverify/Message/PrivateUnverifyWithoutReason", locale].FormatWith(guild.Name, endDateTime)
            : Localization["Unverify/Message/PrivateUnverifyWithReason", locale].FormatWith(guild.Name, endDateTime, profile.Reason);
    }

    public string CreateUpdatePmMessage(IGuild guild, DateTime endDateTime, string locale)
    {
        var formatedEnd = endDateTime.ToCzechFormat();

        return Localization["Unverify/Message/PrivateUpdate", locale].FormatWith(guild.Name, formatedEnd);
    }

    public string CreateUpdateChannelMessage(IGuildUser user, DateTime endDateTime, string locale)
    {
        var username = user.GetDisplayName();
        var formatedEnd = endDateTime.ToCzechFormat();

        return Localization["Unverify/Message/UpdateToChannel", locale].FormatWith(username, formatedEnd);
    }

    public string CreateRemoveAccessManuallyPmMessage(IGuild guild, string locale)
        => Localization["Unverify/Message/PrivateManuallyRemovedUnverify", locale].FormatWith(guild.Name);

    public string CreateRemoveAccessManuallyToChannel(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return Localization["Unverify/Message/ManuallyRemoveToChannel", locale].FormatWith(username);
    }

    public string CreateRemoveAccessManuallyFailed(IGuildUser user, Exception ex, string locale)
    {
        var username = user.GetFullName();

        return Localization["Unverify/Message/ManuallyRemoveFailed", locale].FormatWith(username, ex.Message);
    }

    public string CreateRemoveAccessUnverifyNotFound(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return Localization["Unverify/Message/RemoveAccessUnverifyNotFound", locale].FormatWith(username);
    }

    public string CreateUnverifyFailedToChannel(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return Localization["Unverify/Message/UnverifyFailedToChannel", locale].FormatWith(username);
    }
}
