using Discord;
using System;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Infrastructure.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class EmoteIdAttribute : ValidationAttribute
{
    public EmoteIdAttribute()
    {
        ErrorMessage = @"Emote ID is in an invalid format.";
    }
    
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string val)
            return ValidationResult.Success;

        return !Emote.TryParse(val, out _) ? new ValidationResult(ErrorMessage) : ValidationResult.Success;
    }
}
