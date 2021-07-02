using Discord;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models.Unverify;
using System;

namespace GrillBot.App.Services.Unverify
{
    public static class UnverifyMessageGenerator
    {
        public static string CreateUnverifyMessageToChannel(UnverifyUserProfile profile)
        {
            var endDateTime = profile.End.ToCzechFormat();
            var username = profile.Destination.GetDisplayName();

            return $"Dočasné odebrání přístupu pro uživatele **{username}** bylo dokončeno. Přístup bude navrácen **{endDateTime}**. Důvod: {profile.Reason}";
        }

        public static string CreateUnverifyPMMessage(UnverifyUserProfile profile, IGuild guild)
        {
            var endDateTime = profile.End.ToCzechFormat();

            return $"Byly ti dočasně odebrány všechny práva na serveru **{guild.Name}**. Přístup ti bude navrácen **{endDateTime}**. Důvod: {profile.Reason}";
        }

        public static string CreateUpdatePMMessage(IGuild guild, DateTime endDateTime)
        {
            var formatedEnd = endDateTime.ToCzechFormat();

            return $"Byl ti aktualizován čas pro odebrání práv na serveru **{guild.Name}**. Přístup ti bude navrácen **{formatedEnd}**.";
        }

        public static string CreateUpdateChannelMessage(IGuildUser user, DateTime endDateTime)
        {
            var username = user.GetDisplayName();
            var formatedEnd = endDateTime.ToCzechFormat();

            return $"Reset konce odebrání přístupu pro uživatele **{username}** byl aktualizován.\nPřístup bude navrácen **{formatedEnd}**";
        }

        public static string CreateRemoveAccessManuallyPMMessage(IGuild guild)
        {
            return $"Byl ti předčasně vrácen přístup na serveru **{guild.Name}**";
        }

        public static string CreateRemoveAccessManuallyToChannel(IGuildUser user)
        {
            var username = user.GetDisplayName();

            return $"Předčasné vrácení přístupu pro uživatele **{username}** bylo dokončeno.";
        }

        public static string CreateRemoveAccessManuallyFailed(IGuildUser user, Exception ex)
        {
            var username = user.GetFullName();

            return $"Předčasné vrácení přístupu pro uživatele **{username}** selhalo. ({ex.Message})";
        }

        public static string CreateRemoveAccessUnverifyNotFound(IGuildUser user)
        {
            var username = user.GetDisplayName();

            return $"Předčasné vrácení přístupu pro uživatele **{username}** nelze provést. Unverify nebylo nalezeno.";
        }

        public static string CreateUnverifyFailedToChannel(IGuildUser user)
        {
            var username = user.GetDisplayName();

            return $"Dočasné odebrání přístupu pro uživatele **{username}** se nezdařilo. Uživatel byl obnoven do původního stavu.";
        }
    }
}
