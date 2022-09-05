using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Users;

public class UpdateUserParams : IApiObject
{
    public bool BotAdmin { get; set; }
    public string Note { get; set; }
    public bool WebAdminAllowed { get; set; }
    public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    public bool PublicAdminBlocked { get; set; }
    public bool CommandsDisabled { get; set; }
    public bool PointsDisabled { get; set; }

    public int GetNewFlags(int currentFlags)
    {
        return currentFlags
            .UpdateFlags((int)UserFlags.BotAdmin, BotAdmin)
            .UpdateFlags((int)UserFlags.WebAdmin, WebAdminAllowed)
            .UpdateFlags((int)UserFlags.PublicAdministrationBlocked, PublicAdminBlocked)
            .UpdateFlags((int)UserFlags.CommandsDisabled, CommandsDisabled)
            .UpdateFlags((int)UserFlags.PointsDisabled, PointsDisabled);
    }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(BotAdmin), BotAdmin.ToString() },
            { "NoteLength", Note == null ? "" : Note.Length.ToString() },
            { nameof(WebAdminAllowed), WebAdminAllowed.ToString() },
            { nameof(SelfUnverifyMinimalTime), SelfUnverifyMinimalTime?.ToString("c") },
            { nameof(PublicAdminBlocked), PublicAdminBlocked.ToString() },
            { nameof(CommandsDisabled), CommandsDisabled.ToString() },
            { nameof(PointsDisabled), PointsDisabled.ToString() }
        };
    }
}
