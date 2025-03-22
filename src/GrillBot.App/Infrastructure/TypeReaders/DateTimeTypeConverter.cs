﻿using Discord.Interactions;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Infrastructure.TypeReaders;

public class DateTimeTypeConverter : TypeConverterBase<DateTime>
{
    public override ApplicationCommandOptionType GetDiscordType()
        => ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var value = (string)option.Value;

        if (value.Contains('/') && DateTime.TryParse(value, new CultureInfo("en-US"), DateTimeStyles.None, out var dateTime))
            return Task.FromResult(FromSuccess(dateTime));
        if (DateTime.TryParse(value, new CultureInfo("cs-CZ"), DateTimeStyles.None, out dateTime))
            return Task.FromResult(FromSuccess(dateTime));

        var matchedValue = DateTimeRegexHelper.TryConvert(value);
        if (matchedValue is not null)
            return Task.FromResult(FromSuccess(matchedValue));

        var timeShift = DateTimeRegexHelper.TimeShift().Match(value);
        var timeShiftMatched = timeShift.Success;
        var result = DateTime.Now;

        while (timeShift.Success)
        {
            if (!int.TryParse(timeShift.Groups[1].Value, CultureInfo.InvariantCulture, out var timeValue))
            {
                timeShiftMatched = false;
                break;
            }

            switch (timeShift.Groups[2].Value)
            {
                case "m": // minutes
                    result = result.AddMinutes(timeValue);
                    break;
                case "h": // hours
                    result = result.AddHours(timeValue);
                    break;
                case "d": // days
                    result = result.AddDays(timeValue);
                    break;
                case "w":
                    result = result.AddDays(timeValue * 7);
                    break;
                case "M": // Months
                    result = result.AddMonths(timeValue);
                    break;
                case "r": // Years
                case "y":
                    result = result.AddYears(timeValue);
                    break;
            }

            timeShift = timeShift.NextMatch();
        }

        return timeShiftMatched
            ? Task.FromResult(FromSuccess(result))
            : Task.FromResult(ParseFailed(services, "DateTimeInvalidFormat", context.Interaction.UserLocale));
    }
}
