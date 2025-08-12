using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Unverify;
using GrillBot.Core.Exceptions;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.App.Modules.Interactions.Unverify;

[Group("unverify", "Unverify management")]
[DefaultMemberPermissions(GuildPermission.UseApplicationCommands | GuildPermission.ManageRoles)]
[RequireUserPerms]
public class UnverifyModule(IServiceProvider serviceProvider) : InteractionsModuleBase(serviceProvider)
{
    [SlashCommand("list", "List of all current unverifies")]
    public async Task UnverifyListAsync()
    {
        using var command = await GetCommandAsync<Actions.Commands.Unverify.UnverifyList>();

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
        var handler = new UnverifyListPaginationHandler(page, ServiceProvider!);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("update", "Updates time of an existing unverify")]
    public async Task UpdateUnverifyAsync(IGuildUser user, DateTime newEnd, [Discord.Interactions.MaxLength(500)] string? reason = null)
    {
        using var action = await GetActionAsCommandAsync<Actions.Api.V1.Unverify.UpdateUnverify>();

        try
        {
            var parameters = new UpdateUnverifyParams
            {
                Reason = reason,
                EndAt = newEnd
            };

            var result = await action.Command.ProcessAsync(Context.Guild.Id, user.Id, parameters);
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
        using var action = await GetActionAsCommandAsync<Actions.Api.V1.Unverify.RemoveUnverify>();

        var result = await action.Command.ProcessAsync(Context.Guild.Id, user.Id);
        await SetResponseAsync(result);
    }

    [SlashCommand("set", "Set unverify to user.")]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    public async Task SetUnverifyAsync(DateTime end, string reason, IEnumerable<IUser> users)
    {
        using var command = GetCommand<Actions.Commands.Unverify.SetUnverify>();

        try
        {
            var guildUsers = users.Select(o => o as IGuildUser ?? Context.Guild.GetUser(o.Id)).Where(o => o != null).ToList();
            var result = await command.Command.ProcessAsync(guildUsers, end, reason, false);

            await SetResponseAsync(result[0]);
            foreach (var msg in result.Skip(1)) await ReplyAsync(msg);
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("fun", "Set funverify to user.")]
    public async Task SetFunverifyAsync(DateTime end, string reason, IEnumerable<IUser> users)
    {
        var command = GetCommand<Actions.Commands.Unverify.SetUnverify>();
        var configuration = command.Resolve<IConfiguration>();

        try
        {
            var guildUsers = users.Select(o => o as IGuildUser ?? Context.Guild.GetUser(o.Id)).Where(o => o != null).ToList();
            var result = await command.Command.ProcessAsync(guildUsers, end, reason, true);

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
