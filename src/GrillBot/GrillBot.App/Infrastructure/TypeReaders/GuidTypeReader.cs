using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders
{
    public class GuidTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Guid.TryParse(input, out Guid guid))
                return Task.FromResult(TypeReaderResult.FromSuccess(guid));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Zadaný řetězec není GUID/UUID."));
        }
    }
}
