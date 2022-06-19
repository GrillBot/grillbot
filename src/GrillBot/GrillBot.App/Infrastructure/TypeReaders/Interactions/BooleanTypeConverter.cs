using Discord.Interactions;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions;

public class BooleanTypeConverter : InteractionsTypeConverter<Implementations.BooleanConverter, bool>
{
    protected override async Task<TypeConverterResult> ProcessAsync(Implementations.BooleanConverter converter, string input, IInteractionContext context, IServiceProvider provider)
    {
        var result = await converter.ConvertAsync(input);

        return result == null ? TypeConverterResult.FromError(InteractionCommandError.ParseFailed, "Provided string is not valid boolean value.") : TypeConverterResult.FromSuccess(result);
    }
}
