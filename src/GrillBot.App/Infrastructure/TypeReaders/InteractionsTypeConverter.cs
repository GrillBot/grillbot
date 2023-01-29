using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Implementations;
using GrillBot.Common.Managers.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.TypeReaders;

public abstract class InteractionsTypeConverter<TConverter, TType> : TypeConverter<TType> where TConverter : ConverterBase
{
    public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
    {
        var converter = CreateConverter(context, services);
        var texts = services.GetRequiredService<ITextsManager>();

        return ProcessAsync(converter, option.Value.ToString()!, context, services, texts);
    }

    private static TConverter CreateConverter(IInteractionContext context, IServiceProvider services)
        => (TConverter)Activator.CreateInstance(typeof(TConverter), services, context)!;

    protected abstract Task<TypeConverterResult> ProcessAsync(TConverter converter, string input, IInteractionContext context, IServiceProvider provider, ITextsManager texts);
}
