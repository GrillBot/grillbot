using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Infrastructure.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class DiscordIdAttribute : ValidationAttribute
{
    public const string UnsupportedType = "Nepodporovaný typ Discord identifikátoru.";
    public const string InvalidStringFormat = "Nepodařilo se načíst Discord identifikátor z řetězce.";
    public const string InvalidFormat = "Discord identifikátor není platná hodnota.";

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        return value switch
        {
            null => ValidationResult.Success,
            ulong discordId => Check(discordId, validationContext.MemberName),
            string stringValue => CheckCollection(new List<string> { stringValue }, validationContext),
            List<string> stringValues => CheckCollection(stringValues, validationContext),
            List<ulong> idValues => CheckCollection(idValues.ConvertAll(o => o.ToString()), validationContext),
            _ => new ValidationResult(UnsupportedType, new[] { validationContext.MemberName })
        };
    }

    private static ValidationResult CheckCollection(IReadOnlyList<string> stringValues, ValidationContext context)
    {
        for (var i = 0; i < stringValues.Count; i++)
        {
            var value = stringValues[i].Trim();
            if (string.IsNullOrEmpty(value)) continue;

            var memberName = $"{context.MemberName}.stringValues[{i}]";
            if (!ulong.TryParse(value, out var discordId))
                return new ValidationResult(InvalidStringFormat, new[] { memberName });

            var checkResult = Check(discordId, memberName);
            if (checkResult != ValidationResult.Success)
                return checkResult;
        }

        return ValidationResult.Success;
    }

    private static ValidationResult Check(ulong id, string memberName)
    {
        if (id == 0 || (id >> 22) == 0)
            return new ValidationResult(InvalidFormat, new[] { memberName });

        return ValidationResult.Success;
    }
}
