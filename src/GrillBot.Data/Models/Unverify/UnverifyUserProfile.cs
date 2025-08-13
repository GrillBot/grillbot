﻿using Discord;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.Unverify;

public class UnverifyUserProfile
{
    public IGuildUser Destination { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<IRole> RolesToRemove { get; set; }
    public List<IRole> RolesToKeep { get; set; }
    public List<ChannelOverride> ChannelsToKeep { get; set; }
    public List<ChannelOverride> ChannelsToRemove { get; set; }
    public string? Reason { get; set; }
    public bool IsSelfUnverify { get; set; }
    public string Language { get; set; }
    public bool KeepMutedRole { get; set; }

    public UnverifyUserProfile(IGuildUser destination, DateTime start, DateTime end, bool isSelfUnverify, string language)
    {
        Destination = destination;
        Start = start;
        End = end;
        IsSelfUnverify = isSelfUnverify;
        Language = language;

        RolesToKeep = new List<IRole>();
        RolesToRemove = new List<IRole>();
        ChannelsToKeep = new List<ChannelOverride>();
        ChannelsToRemove = new List<ChannelOverride>();
    }
}
