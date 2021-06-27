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

        private Dictionary<Func<Regex>, Func<DateTime>> MatchingFunctions { get; } = new Dictionary<Func<Regex>, Func<DateTime>>()
        {
            { () => new Regex("^(today|dnesk?a?)$", regexOptions), () => DateTime.Today }, // today, dnes, dneska
            { () => new Regex("^(tommorow|z[i|í]tra|za[j|v]tra)$", regexOptions), () => DateTime.Now.AddDays(1) }, // tommorow, zítra, zitra, zajtra, zavtra
            { () => new Regex("^(v[c|č]era|yesterday|vchora)$", regexOptions), () => DateTime.Now.AddDays(-1) }, // vcera, včera, yesterday, vchora
            { () => new Regex("^(poz[i|í]t[r|ř][i|í]|pozajtra|pislyazavtra)$", regexOptions), () => DateTime.Now.AddDays(2) }, // pozítří, pozitri, pozajtra, pislyazavtra
            { () => new Regex("^(te[ď|d]|now|[te|za]+raz)$", regexOptions), () => DateTime.Now } // teď, ted, now, teraz, zaraz
        };

        private List<CultureInfo> SupportedCultures { get; } = new List<CultureInfo>()
        {
            new CultureInfo("cs-CZ"),
            new CultureInfo("en-US"),
            CultureInfo.InvariantCulture
        };

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            foreach (var culture in SupportedCultures)
            {
                if (DateTime.TryParse(input, culture, DateTimeStyles.None, out DateTime dateTime))
                    return Task.FromResult(TypeReaderResult.FromSuccess(dateTime));
            }

            foreach (var func in MatchingFunctions)
            {
                var regex = func.Key();

                if (regex.IsMatch(input))
                    return Task.FromResult(TypeReaderResult.FromSuccess(func.Value()));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Datum a čas není ve správném formátu."));
        }
    }
}
