using GrillBot.Common.Managers.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public abstract class ConverterBase
{
    protected IServiceProvider ServiceProvider { get; }
    protected IDiscordClient Client { get; }
    protected IGuild? Guild { get; }
    protected IMessageChannel Channel { get; }
    protected IUser User { get; }
    private IDiscordInteraction Interaction { get; }
    private ITextsManager Texts { get; }

    private ConverterBase(IServiceProvider provider, IDiscordClient client, IGuild guild, IMessageChannel channel, IUser user, IDiscordInteraction interaction)
    {
        ServiceProvider = provider;
        Client = client;
        Guild = guild;
        Channel = channel;
        User = user;
        Interaction = interaction;
        Texts = ServiceProvider.GetRequiredService<ITextsManager>();
    }

    protected ConverterBase(IServiceProvider provider, IInteractionContext context)
        : this(provider, context.Client, context.Guild, context.Channel, context.User, context.Interaction)
    {
    }

    protected string GetLocalizedText(string id)
        => Texts[$"TypeConverters/{id}", Interaction.UserLocale];
}

public abstract class ConverterBase<TResult> : ConverterBase
{
    protected ConverterBase(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
    }

    public virtual Task<TResult?> ConvertAsync(string value)
    {
        return Task.FromResult(default(TResult));
    }
}
