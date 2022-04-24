using Discord.Commands;
using GrillBot.Data.Extensions;
using System.Text.RegularExpressions;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class DateTimeConverter : ConverterBase<DateTime>
{
    private const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace;

    public DateTimeConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public DateTimeConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    private Dictionary<Regex, Func<DateTime>> MatchingFunctions { get; } = new()
    {
        { new Regex("^(today|dnes(ka)?)$", regexOptions), () => DateTime.Today }, // today, dnes, dneska
        { new Regex("^(tommorow|z[ií]tra|za[jv]tra)$", regexOptions), () => DateTime.Now.AddDays(1) }, // tommorow, zítra, zitra, zajtra, zavtra
        { new Regex("^(v[cč]era|yesterday|vchora)$", regexOptions), () => DateTime.Now.AddDays(-1) }, // vcera, včera, yesterday, vchora
        { new Regex("^(poz[ií]t[rř][ií]|pozajtra|poslezavtra)$", regexOptions), () => DateTime.Now.AddDays(2) }, // pozítří, pozitri, pozajtra, poslezavtra
        { new Regex("^(^(te[dď]|now|(te|za)raz)$)$", regexOptions), () => DateTime.Now } // teď, ted, now, teraz, zaraz
    };

    private Regex TimeShiftRegex { get; } = new(@"(\d+)(m|h|d|M|y|r)", regexOptions);

    public override Task<DateTime> ConvertAsync(string value)
    {
        if (value.Contains('/') && DateTime.TryParse(value, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime dateTime)) return Task.FromResult(dateTime);
        if (DateTime.TryParse(value, new CultureInfo("cs-CZ"), DateTimeStyles.None, out dateTime)) return Task.FromResult(dateTime);

        var matchedFunction = MatchingFunctions.FirstOrDefault(o => o.Key.IsMatch(value));
        if (matchedFunction.Key != null) return Task.FromResult(matchedFunction.Value());

        var timeShift = TimeShiftRegex.Match(value);
        var timeShiftMatched = timeShift.Success;
        var result = DateTime.Now;

        while (timeShift.Success)
        {
            var timeValue = timeShift.Groups[1].Value.ToInt();

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
                case "r":
                case "y":
                    result = result.AddYears(timeValue);
                    break;
            }

            timeShift = timeShift.NextMatch();
        }

        if (!timeShiftMatched)
            return Task.FromException<DateTime>(new InvalidOperationException("Datum a čas není ve správném formátu."));

        return Task.FromResult(result);
    }
}
