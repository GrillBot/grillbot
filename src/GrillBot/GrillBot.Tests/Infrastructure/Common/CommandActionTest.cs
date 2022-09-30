using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Discord;
using GrillBot.App.Actions;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Infrastructure.Common;

[ExcludeFromCodeCoverage]
public abstract class CommandActionTest<TAction> : ActionTest<TAction> where TAction : CommandAction
{
    protected CommandConfigurationAttribute Configuration
        => GetMethod().GetCustomAttribute<CommandConfigurationAttribute>();

    protected virtual IGuild Guild { get; }
    protected virtual IGuildUser User { get; }
    protected virtual IDiscordInteraction Interaction { get; }
    protected IInteractionContext Context { get; private set; }

    protected override bool CanInitProvider
        => Configuration?.CanInitProvider ?? false;

    protected override void Init()
    {
        Context = new InteractionContextBuilder().SetGuild(Guild).SetUser(User).SetInteraction(Interaction).Build();
    }
}
