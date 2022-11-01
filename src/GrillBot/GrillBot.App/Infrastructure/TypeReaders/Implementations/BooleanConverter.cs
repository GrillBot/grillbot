using System.Text.RegularExpressions;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class BooleanConverter : ConverterBase<bool?>
{
    public BooleanConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    private Dictionary<Regex, bool> MatchingFunctions { get; } = new()
    {
        { new Regex("^(ano|yes|true?)$"), true }, // ano, ne, true, tru
        { new Regex("^(ne|no|false?)$"), false } // ne, no, false, fals
    };

    public override Task<bool?> ConvertAsync(string value)
    {
        if (bool.TryParse(value, out var result))
            return Task.FromResult<bool?>(result);

        var match = MatchingFunctions.FirstOrDefault(o => o.Key.IsMatch(value ?? ""));
        return match.Key != null ? Task.FromResult<bool?>(match.Value) : Task.FromResult<bool?>(null);
    }
}
