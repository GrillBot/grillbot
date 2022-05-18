using GrillBot.Database.Enums;
using System;

namespace GrillBot.Data.Models.API.Users;

public class UpdateUserParams
{
    public bool BotAdmin { get; set; }
    public string Note { get; set; }
    public bool WebAdminAllowed { get; set; }
    public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    public bool PublicAdminBlocked { get; set; }
    public bool CommandsDisabled { get; set; }

    public int GetNewFlags(int currentFlags)
    {
        int newFlags = currentFlags;

        if (BotAdmin) newFlags |= (int)UserFlags.BotAdmin;
        else newFlags &= ~(int)UserFlags.BotAdmin;

        if (WebAdminAllowed) newFlags |= (int)UserFlags.WebAdmin;
        else newFlags &= ~(int)UserFlags.WebAdmin;

        if (PublicAdminBlocked) newFlags |= (int)UserFlags.PublicAdministrationBlocked;
        else newFlags &= ~(int)UserFlags.PublicAdministrationBlocked;

        if (CommandsDisabled) newFlags |= (int)UserFlags.CommandsDisabled;
        else newFlags &= ~(int)UserFlags.CommandsDisabled;

        return newFlags;
    }
}
