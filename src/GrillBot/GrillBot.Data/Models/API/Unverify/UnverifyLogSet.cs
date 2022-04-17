using AutoMapper;
using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify;

public class UnverifyLogSet
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<Role> RolesToKeep { get; set; }
    public List<Role> RolesToRemove { get; set; }
    public List<string> ChannelIdsToKeep { get; set; }
    public List<string> ChannelIdsToRemove { get; set; }
    public string Reason { get; set; }
    public bool IsSelfUnverify { get; set; }
}
