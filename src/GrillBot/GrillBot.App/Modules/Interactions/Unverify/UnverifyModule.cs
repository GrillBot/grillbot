using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Unverify;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Unverify;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Interactions.Unverify;

[Group("unverify", "Unverify management")]
[DefaultMemberPermissions(GuildPermission.UseApplicationCommands | GuildPermission.ManageRoles)]
[RequireUserPerms(GuildPermission.ManageRoles)]
[ExcludeFromCodeCoverage]
public class UnverifyModule : InteractionsModuleBase
{
    public UnverifyModule(IServiceProvider serviceProvider) : base(serviceProvider)
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

    [SlashCommand("set", "Set unverify to user.")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task SetUnverifyAsync(DateTime end, string reason, IGuildUser user, IGuildUser user2 = null, IGuildUser user3 = null, IGuildUser user4 = null, IGuildUser user5 = null)
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<UnverifyService>();

        try
        {
            var users = new[] { user, user2, user3, user4, user5 }.Where(o => o != null).ToList();
            var result = await service.SetUnverifyAsync(users, end, reason, Context.Guild, Context.User, false, Locale);

            await SetResponseAsync(result[0]);
            foreach (var msg in result.Skip(1)) await ReplyAsync(msg);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("fun", "Set funverify to user.")]
    public async Task SetFunverifyAsync(DateTime end, string reason, IGuildUser user, IGuildUser user2 = null, IGuildUser user3 = null, IGuildUser user4 = null, IGuildUser user5 = null)
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<UnverifyService>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            var users = new[] { user, user2, user3, user4, user5 }.Where(o => o != null).ToList();
            var result = await service.SetUnverifyAsync(users, end, reason, Context.Guild, Context.User, true, Locale);

            await SetResponseAsync(result[0]);
            foreach (var msg in result.Skip(1)) await ReplyAsync(msg);

            await Task.Delay(configuration.GetValue<int>("Unverify:FunverifySleepTime"));
            await Context.Channel.SendMessageAsync(configuration["Discord:Emotes:KappaLul"]);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
