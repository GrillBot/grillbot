using GrillBot.App.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.User;

public class UserAccessListHandler : ComponentInteractionHandler
{
    private IServiceProvider ServiceProvider { get; }
    private int Page { get; }

    public UserAccessListHandler(IServiceProvider serviceProvider, int page)
    {
        ServiceProvider = serviceProvider;
        Page = page;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<UserAccessListMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var forUser = await context.Guild.GetUserAsync(metadata.ForUserId);
        if (forUser == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        using var scope = ServiceProvider.CreateScope();
        var action = scope.ServiceProvider.GetRequiredService<Actions.Commands.UserAccessList>();
        action.Init(context);

        var pagesCount = await action.ComputePagesCount(forUser);
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var (embed, paginationComponents) = await action.ProcessAsync(forUser, newPage);
        await component.UpdateAsync(msg =>
        {
            msg.Components = paginationComponents;
            msg.Embed = embed;
        });
    }
}
