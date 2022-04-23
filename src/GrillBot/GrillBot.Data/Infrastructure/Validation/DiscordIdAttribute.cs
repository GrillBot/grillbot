using System;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Infrastructure.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DiscordIdAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        ulong discordId;

        if (value == null)
            return ValidationResult.Success;

        if (value is ulong _discordId)
        {
            discordId = _discordId;
        }
        else if (value is string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue.Trim()))
                return ValidationResult.Success;

            if (!ulong.TryParse(stringValue.Trim(), out discordId))
                return new ValidationResult("Discord identifikátor není celočíselná hodnota typu UInt64.");
        }
        else
        {
            return new ValidationResult("Nepodporaný typ Discord identifikátoru.");
        }

        if (discordId == 0 || (discordId >> 22) == 0)
            return new ValidationResult("Discord identifikátor není platná hodnota.");

        return ValidationResult.Success;
    }
}
