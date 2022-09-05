using System.Collections.Generic;
using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Permissions;

public class CreateExplicitPermissionParams : IApiObject
{
    [Required]
    public string Command { get; set; }

    [Required]
    public bool IsRole { get; set; }

    [Required]
    public string TargetId { get; set; }

    [Required]
    public ExplicitPermissionState State { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(Command), Command },
            { nameof(IsRole), IsRole.ToString() },
            { nameof(TargetId), TargetId },
            { nameof(State), $"{State} ({(int)State})" }
        };
    }
}
