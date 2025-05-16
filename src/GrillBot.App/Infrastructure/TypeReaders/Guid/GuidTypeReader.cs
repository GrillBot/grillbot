using Discord.Interactions;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Infrastructure.TypeReaders.Guid;

public class GuidTypeReader : TypeReader<System.Guid>
{
    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services)
    {
        return System.Guid.TryParse(option, CultureInfo.InvariantCulture, out var guid) ?
            Task.FromResult(TypeReaderHelper.FromSuccess(guid))
            : Task.FromResult(TypeReaderHelper.ParseFailed(services, "GuidInvalidValue", context.Interaction.UserLocale));
    }
}
