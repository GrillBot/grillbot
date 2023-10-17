﻿using System;

namespace GrillBot.Data.Models.Unverify;

public class UnverifyLogUpdate
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Reason { get; set; }
}
