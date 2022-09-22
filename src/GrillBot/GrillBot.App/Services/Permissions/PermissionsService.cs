using GrillBot.App.Services.Permissions.Models;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Commands = Discord.Commands;
using Interactions = Discord.Interactions;

namespace GrillBot.App.Services.Permissions;

public class PermissionsService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IServiceProvider ServiceProvider { get; }

    public PermissionsService(GrillBotDatabaseBuilder databaseBuilder, IServiceProvider serviceProvider)
    {
        DatabaseBuilder = databaseBuilder;
        ServiceProvider = serviceProvider;
    }

    public async Task<string> CheckPermissionsAsync(CheckRequestBase request)
    {
        request.FixImplicitPermissions();

        if (CheckContext(request) == false)
            return "Voláš příkaz tam, kde jej nelze spustit.";

        await using var repository = DatabaseBuilder.CreateRepository();
        var user = await repository.User.FindUserAsync(request.User, true);

        if (user?.HaveFlags(UserFlags.BotAdmin) ?? false)
            return null;
        if (await CheckChannelDisabledAsync(repository, request))
            return "V tomto kanálu byly příkazy deaktivovány.";
        if ((user?.HaveFlags(UserFlags.CommandsDisabled) ?? false) || await CheckExplicitBansAsync(repository, request))
            return "Byl ti zakázán přístup k tomuto příkazu.";
        if (await CheckExplicitAllowAsync(repository, request) == true || CheckServerBooster(request) == true)
            return null;
        if (await CheckGuildPermissionsAsync(request) == true)
            return null;

        return await CheckChannelPermissionsAsync(request) == true ? null : "Nesplňuješ podmínky pro spuštění příkazu na serveru.";
    }

    private static async Task<bool> CheckChannelDisabledAsync(GrillBotRepository repository, CheckRequestBase request)
    {
        var channelId = request.Channel is IThreadChannel { CategoryId: { } } thread
            ? thread.CategoryId.Value
            : request.Channel.Id;

        var channel = await repository.Channel.FindChannelByIdAsync(channelId, request.Guild?.Id, true);
        return channel?.HasFlag(ChannelFlags.CommandsDisabled) ?? false;
    }

    private static bool? CheckContext(CheckRequestBase request)
    {
        if (request is InteractionsCheckRequest) return true; // Interactions are registered only to guilds.

        var checkRequest = (CommandsCheckRequest)request;
        if (checkRequest.Context == null) return null; // Command not depend on the context (Guild/DMs).

        // Command allows only DMs or only Guilds.
        return (checkRequest.Context == Commands.ContextType.DM && request.Guild == null) ||
               (checkRequest.Context == Commands.ContextType.Guild && request.Guild != null);
    }

    private async Task<bool?> CheckChannelPermissionsAsync(CheckRequestBase request)
    {
        if (request.ChannelPermissions == null) return null; // Command not depend on the channel permissions.

        var commandsRequest = request as CommandsCheckRequest;
        foreach (var perm in request.ChannelPermissions)
        {
            if (commandsRequest == null)
            {
                var checkRequest = (InteractionsCheckRequest)request;
                var attribute = new Interactions.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckRequirementsAsync(checkRequest.InteractionContext, checkRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
            else
            {
                var attribute = new Commands.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckPermissionsAsync(commandsRequest.CommandContext, commandsRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
        }

        return true;
    }

    private async Task<bool?> CheckGuildPermissionsAsync(CheckRequestBase request)
    {
        if (request.GuildPermissions == null) return null; // Command not depend on the guild permissions.

        var commandsRequest = request as CommandsCheckRequest;
        foreach (var perm in request.GuildPermissions)
        {
            if (commandsRequest == null)
            {
                var checkRequest = (InteractionsCheckRequest)request;
                var attribute = new Interactions.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckRequirementsAsync(checkRequest.InteractionContext, checkRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
            else
            {
                var attribute = new Commands.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckPermissionsAsync(commandsRequest.CommandContext, commandsRequest.CommandInfo, ServiceProvider);

                if (!result.IsSuccess)
                    return false;
            }
        }

        return true;
    }

    private static bool? CheckServerBooster(CheckRequestBase request)
    {
        if (!request.AllowBooster) return null;

        return request.User is IGuildUser user && user.GetRoles().Any(o => o.Tags?.IsPremiumSubscriberRole == true);
    }

    private static async Task<bool?> CheckExplicitAllowAsync(GrillBotRepository repository, CheckRequestBase request)
    {
        var permissions = await repository.Permissions.GetAllowedPermissionsForCommand(request.CommandName);
        if (permissions.Count == 0)
            return null;

        if (permissions.Any(o => !o.IsRole && o.TargetId == request.User.Id.ToString()))
            return true; // Explicit allow permission for user.

        return request.User is IGuildUser user && user.RoleIds.Any(roleId => permissions.Any(x => x.IsRole && x.TargetId == roleId.ToString()));
    }

    private static async Task<bool> CheckExplicitBansAsync(GrillBotRepository repository, CheckRequestBase request)
    {
        return await repository.Permissions.ExistsBannedCommandForUser(request.CommandName, request.User);
    }
}
