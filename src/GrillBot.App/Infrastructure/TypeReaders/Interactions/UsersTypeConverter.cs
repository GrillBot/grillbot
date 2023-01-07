using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Infrastructure.TypeReaders.Interactions;

public class UsersTypeConverter : InteractionsTypeConverter<UsersConverter, IEnumerable<IUser>>
{
    protected override async Task<TypeConverterResult> ProcessAsync(UsersConverter converter, string input, IInteractionContext context, IServiceProvider provider, ITextsManager texts)
    {
        try
        {
            var result = await converter.ConvertAsync(input);
            return TypeConverterResult.FromSuccess(result.ToArray());
        }
        catch (NotFoundException ex)
        {
            return TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, ex.Message);
        }
        catch (FormatException ex)
        {
            return TypeConverterResult.FromError(InteractionCommandError.ParseFailed, ex.Message);
        }
    }
}
