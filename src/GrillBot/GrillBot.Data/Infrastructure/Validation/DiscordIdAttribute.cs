using System;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Infrastructure.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class DiscordIdAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        ulong discordId = 0;

        switch (value)
        {
            case null:
                return ValidationResult.Success;
            case ulong id:
                discordId = id;
                break;
            case string stringValue when string.IsNullOrEmpty(stringValue.Trim()):
                return ValidationResult.Success;
            case string stringValue when !ulong.TryParse(stringValue.Trim(), out discordId):
                return new ValidationResult("Discord identifikátor není celočíselná hodnota typu UInt64.");
            case string:
                break;
            default:
                return new ValidationResult("Nepodporaný typ Discord identifikátoru.");
        }

        if (discordId == 0 || (discordId >> 22) == 0)
            return new ValidationResult("Discord identifikátor není platná hodnota.");

        return ValidationResult.Success;
    }
}
