using Discord;
using System;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Infrastructure.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public class EmoteIdAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string val)
            return ValidationResult.Success;

        if (!Emote.TryParse(val, out var _))
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}
