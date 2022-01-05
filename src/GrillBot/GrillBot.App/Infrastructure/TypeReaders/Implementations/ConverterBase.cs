using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public abstract class ConverterBase
{
    protected IServiceProvider ServiceProvider { get; }
    protected IDiscordClient Client { get; }
    protected IGuild Guild { get; }
    protected IMessageChannel Channel { get; }
    protected IUser User { get; }
    protected IUserMessage UserMessage { get; }
    protected IDiscordInteraction Interaction { get; }

    private ConverterBase(IServiceProvider provider, IDiscordClient client, IGuild guild, IMessageChannel channel,
        IUser user, IUserMessage message, IDiscordInteraction interaction)
    {
        ServiceProvider = provider;
        Client = client;
        Guild = guild;
        Channel = channel;
        User = user;
        UserMessage = message;
        Interaction = interaction;
    }

    protected ConverterBase(IServiceProvider provider, ICommandContext context)
        : this(provider, context?.Client, context?.Guild, context?.Channel, context?.User, context?.Message, null)
    {
    }

    protected ConverterBase(IServiceProvider provider, IInteractionContext context)
        : this(provider, context?.Client, context?.Guild, context?.Channel, context?.User, null, context?.Interaction)
    {
    }
}

public abstract class ConverterBase<TResult> : ConverterBase
{
    protected ConverterBase(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    protected ConverterBase(IServiceProvider provider, IInteractionContext context) : base(provider, context) { }

    public virtual Task<TResult> ConvertAsync(string value) { return Task.FromResult(default(TResult)); }
}
