using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.UserMeasures;

public class CreateUserMeasuresWarningParams : IDictionaryObject
{
    [Required]
    [DiscordId]
    [StringLength(32)]
    public string GuildId { get; set; } = null!;

    [Required]
    [DiscordId]
    [StringLength(32)]
    public string UserId { get; set; } = null!;

    [Required]
    public string Message { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(UserId), UserId },
            { "MessageLength", Message.Length.ToString() }
        };
    }
}
