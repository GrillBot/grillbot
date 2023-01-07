using System;

namespace GrillBot.Data.Models.API.Invites;

public class InviteBase
{
    public string Code { get; set; }
    public DateTime? CreatedAt { get; set; }
}
