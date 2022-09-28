using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Unverify;

public class UpdateUnverifyParams : IApiObject
{
    [Required]
    public DateTime EndAt { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(EndAt), EndAt.ToString("o") }
        };
    }
}
