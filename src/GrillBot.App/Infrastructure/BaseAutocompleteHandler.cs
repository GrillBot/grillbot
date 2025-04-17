using Discord.Interactions;
using GrillBot.App.Managers.Auth;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure;

public abstract class BaseAutocompleteHandler : AutocompleteHandler
{
    protected static async Task<ScopedCommand<TCommand>> CreateCommandAsync<TCommand>(IInteractionContext context, IServiceProvider serviceProvider) where TCommand : notnull
    {
        var command = new ScopedCommand<TCommand>(serviceProvider.CreateScope());

        var jwtToken = await command.Resolve<JwtTokenManager>()
            .CreateTokenForUserAsync(context.User, TextsManager.FixLocale(context.Interaction.UserLocale), context);
        if (!string.IsNullOrEmpty(jwtToken?.AccessToken))
            command.Resolve<ICurrentUserProvider>().SetCustomToken(jwtToken.AccessToken);

        return command;
    }
}
