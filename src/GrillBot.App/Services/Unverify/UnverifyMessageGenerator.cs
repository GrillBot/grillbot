using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.Unverify;

namespace GrillBot.App.Services.Unverify;

public class UnverifyMessageGenerator
{
    private ITextsManager Texts { get; }

    public UnverifyMessageGenerator(ITextsManager texts)
    {
        Texts = texts;
    }

    public string CreateUnverifyMessageToChannel(UnverifyUserProfile profile, string locale)
    {
        var endDateTime = profile.End.ToCzechFormat();
        var username = profile.Destination.GetDisplayName();

        return profile.IsSelfUnverify
            ? Texts["Unverify/Message/UnverifyToChannelWithoutReason", locale].FormatWith(username, endDateTime)
            : Texts["Unverify/Message/UnverifyToChannelWithReason", locale].FormatWith(username, endDateTime, profile.Reason);
    }

    public string CreateUnverifyPmMessage(UnverifyUserProfile profile, IGuild guild, string locale)
    {
        var endDateTime = profile.End.ToCzechFormat();

        return profile.IsSelfUnverify
            ? Texts["Unverify/Message/PrivateUnverifyWithoutReason", locale].FormatWith(guild.Name, endDateTime)
            : Texts["Unverify/Message/PrivateUnverifyWithReason", locale].FormatWith(guild.Name, endDateTime, profile.Reason);
    }

    public string CreateUpdatePmMessage(IGuild guild, DateTime endDateTime, string? reason, string locale)
    {
        var formatedEnd = endDateTime.ToCzechFormat();

        var textId = string.IsNullOrEmpty(reason) ? "PrivateUpdate" : "PrivateUpdateWithReason";
        return Texts[$"Unverify/Message/{textId}", locale].FormatWith(guild.Name, formatedEnd, reason);
    }

    public string CreateUpdateChannelMessage(IGuildUser user, DateTime endDateTime, string? reason, string locale)
    {
        var username = user.GetDisplayName();
        var formatedEnd = endDateTime.ToCzechFormat();

        var textId = string.IsNullOrEmpty(reason) ? "UpdateToChannel" : "UpdateToChannelWithReason";
        return Texts[$"Unverify/Message/{textId}", locale].FormatWith(username, formatedEnd, reason);
    }

    public string CreateRemoveAccessManuallyPmMessage(IGuild guild, string locale)
        => Texts["Unverify/Message/PrivateManuallyRemovedUnverify", locale].FormatWith(guild.Name);

    public string CreateRemoveAccessManuallyToChannel(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return Texts["Unverify/Message/ManuallyRemoveToChannel", locale].FormatWith(username);
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

    public string CreateUnverifyFailedToChannel(IGuildUser user, string locale)
    {
        var username = user.GetDisplayName();

        return Texts["Unverify/Message/UnverifyFailedToChannel", locale].FormatWith(username);
    }
}
