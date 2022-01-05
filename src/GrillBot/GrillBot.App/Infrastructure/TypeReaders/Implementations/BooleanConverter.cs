using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class BooleanConverter : ConverterBase<bool?>
{
    public BooleanConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public BooleanConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    private Dictionary<Regex, bool> MatchingFunctions { get; } = new Dictionary<Regex, bool>()
    {
        { new Regex("^(ano|yes|true?)$"), true }, // ano, ne, true, tru
        { new Regex("^(ne|no|false?)$"), false } // ne, no, false, fals
    };

    public override Task<bool?> ConvertAsync(string value)
    {
        if (bool.TryParse(value, out bool result))
            return Task.FromResult<bool?>(result);

        var match = MatchingFunctions.FirstOrDefault(o => o.Key.IsMatch(value));
        if (match.Key != null) return Task.FromResult<bool?>(match.Value);

        return Task.FromResult<bool?>(null);
    }
}
