using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class DateTimeTypeReader : TypeReader
    {
        private const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;

        private Dictionary<Regex, Func<DateTime>> MatchingFunctions { get; } = new()
        {
            { new Regex("^(today|dnes(ka)?)$", regexOptions), () => DateTime.Today }, // today, dnes, dneska
            { new Regex("^(tommorow|z[ií]tra|za[jv]tra)$", regexOptions), () => DateTime.Now.AddDays(1) }, // tommorow, zítra, zitra, zajtra, zavtra
            { new Regex("^(v[cč]era|yesterday|vchora)$", regexOptions), () => DateTime.Now.AddDays(-1) }, // vcera, včera, yesterday, vchora
            { new Regex("^(poz[ií]t[rř][ií]|pozajtra|poslezavtra)$", regexOptions), () => DateTime.Now.AddDays(2) }, // pozítří, pozitri, pozajtra, poslezavtra
            { new Regex("^(^(te[dď]|now|(te|za)raz)$)$", regexOptions), () => DateTime.Now } // teď, ted, now, teraz, zaraz
        };

        private Regex TimeShiftRegex { get; } = new(@"^(\d+)(m|h|d|M|y)$", regexOptions);

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            // US dates use '/' as delimeter. We use this fact to detect american dates and parse them correctly (MM/DD instead of DD.MM)
            if (input.Contains('/') && DateTime.TryParse(input, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime dateTime))
                return Task.FromResult(TypeReaderResult.FromSuccess(dateTime));

            if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                return Task.FromResult(TypeReaderResult.FromSuccess(dateTime));

            foreach (var func in MatchingFunctions)
            {
                if (func.Key.IsMatch(input))
                    return Task.FromResult(TypeReaderResult.FromSuccess(func.Value()));
            }

            var timeShift = TimeShiftRegex.Match(input);
            if (timeShift.Success)
            {
                var result = DateTime.Now;
                var timeValue = Convert.ToInt32(timeShift.Groups[1].Value);

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
                    case "M":
                        result = result.AddMonths(timeValue);
                        break;
                    case "y":
                        result = result.AddYears(timeValue);
                        break;
                }

                return Task.FromResult(TypeReaderResult.FromSuccess(result));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Datum a čas není ve správném formátu."));
        }
    }
}
