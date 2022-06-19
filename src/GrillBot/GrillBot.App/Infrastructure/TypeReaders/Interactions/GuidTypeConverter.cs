using Discord.Interactions;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions;

public class GuidTypeConverter : InteractionsTypeConverter<Implementations.GuidConverter, Guid>
{
    protected override async Task<TypeConverterResult> ProcessAsync(Implementations.GuidConverter converter, string input, IInteractionContext context, IServiceProvider provider)
    {
        var result = await converter.ConvertAsync(input);
        return result != null ? TypeConverterResult.FromSuccess(result) : TypeConverterResult.FromError(InteractionCommandError.ParseFailed, "Zadaný řetězec není GUID/UUID.");
    }
}
