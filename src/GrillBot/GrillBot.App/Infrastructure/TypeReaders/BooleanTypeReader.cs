using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class BooleanTypeReader : TypeReader
    {
        private Dictionary<Regex, bool> MatchingFunctions { get; } = new Dictionary<Regex, bool>()
        {
            { new Regex("^(ano|yes|true?)$"), true }, // ano, ne, true, tru
            { new Regex("^(ne|no|false?)$"), false } // ne, no, false, fals
        };

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (bool.TryParse(input, out bool result))
                return Task.FromResult(TypeReaderResult.FromSuccess(result));

            foreach (var func in MatchingFunctions)
            {
                if (func.Key.IsMatch(input))
                    return Task.FromResult(TypeReaderResult.FromSuccess(func.Value));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Provided string is not valid boolean value."));
        }
    }
}
