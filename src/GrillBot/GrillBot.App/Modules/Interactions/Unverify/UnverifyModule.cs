using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Unverify;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.App.Modules.Interactions.Unverify;

[Group("unverify", "Unverify management")]
[DefaultMemberPermissions(GuildPermission.UseApplicationCommands | GuildPermission.ManageRoles)]
[RequireUserPerms(GuildPermission.ManageRoles)]
[ExcludeFromCodeCoverage]
public class UnverifyModule : InteractionsModuleBase
{
    public UnverifyModule(IServiceProvider serviceProvider) : base(null, serviceProvider)
    {
    }

    [SlashCommand("list", "List of all current unverifies")]
    public async Task UnverifyListAsync()
    {
        using var command = GetCommand<Actions.Commands.Unverify.UnverifyList>();

        try
        {
            var (embed, paginationComponent) = await command.Command.ProcessAsync(0);
            await SetResponseAsync(embed: embed, components: paginationComponent);
        }
        catch (NotFoundException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("unverify:*", ignoreGroupNames: true)]
    public async Task HandleUnverifyListPaginationAsync(int page)
    {
        var handler = new UnverifyListPaginationHandler(page, ServiceProvider);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("update", "Updates time of an existing unverify")]
    public async Task UpdateUnverifyAsync(IGuildUser user, DateTime newEnd)
    {
        using var action = GetActionAsCommand<Actions.Api.V1.Unverify.UpdateUnverify>();

        try
        {
            var result = await action.Command.ProcessAsync(Context.Guild.Id, user.Id, new UpdateUnverifyParams { EndAt = newEnd });
            await SetResponseAsync(result);
        }
        catch (Exception ex)
        {
            if (ex is NotFoundException or ValidationException)
                await SetResponseAsync(ex.Message);

            throw;
        }
    }

    [SlashCommand("remove", "Remove an active unverify.")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task RemoveUnverifyAsync(IGuildUser user)
    {
        using var action = GetActionAsCommand<Actions.Api.V1.Unverify.RemoveUnverify>();

        var result = await action.Command.ProcessAsync(Context.Guild.Id, user.Id);
        await SetResponseAsync(result);
    }
}
