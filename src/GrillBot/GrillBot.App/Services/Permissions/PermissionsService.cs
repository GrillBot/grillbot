using GrillBot.App.Services.Permissions.Models;
using GrillBot.Database.Enums;
using Commands = Discord.Commands;
using Interactions = Discord.Interactions;

namespace GrillBot.App.Services.Permissions;

public class PermissionsService
{
    private GrillBotDatabaseBuilder DbFactory { get; }
    private IServiceProvider ServiceProvider { get; }

    public PermissionsService(GrillBotDatabaseBuilder dbFactory, IServiceProvider serviceProvider)
    {
        DbFactory = dbFactory;
        ServiceProvider = serviceProvider;
    }

    public async Task<PermsCheckResult> CheckPermissionsAsync(CheckRequestBase request)
    {
        request.FixImplicitPermissions();

        using var dbContext = DbFactory.Create();

        var user = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.User.Id.ToString());

        return new PermsCheckResult()
        {
            ChannelDisabled = await CheckChannelDisabledAsync(dbContext, request),
            ContextCheck = CheckContext(request),
            ChannelPermissions = await CheckChannelPermissionsAsync(request),
            GuildPermissions = await CheckGuildPermissionsAsync(request),
            BoosterAllowed = CheckServerBooster(request),
            IsAdmin = user.HaveFlags(UserFlags.BotAdmin),
            ExplicitAllow = await CheckExplicitAllowAsync(dbContext, request),
            ExplicitBan = user.HaveFlags(UserFlags.CommandsDisabled) || await CheckExplicitBansAsync(dbContext, request)
        };
    }

    private static Task<bool> CheckChannelDisabledAsync(GrillBotContext dbContext, CheckRequestBase request)
    {
        ulong channelId = request.Channel is IThreadChannel thread ? thread.CategoryId.Value : request.Channel.Id;

        var query = dbContext.Channels.AsNoTracking()
            .Where(o => o.ChannelId == channelId.ToString() && (o.Flags & (int)ChannelFlags.CommandsDisabled) != 0);

        if (request.Guild != null)
            query = query.Where(o => o.GuildId == request.Guild.Id.ToString());

        return query.AnyAsync();
    }

    private static bool? CheckContext(CheckRequestBase request)
    {
        if (request is InteractionsCheckRequest) return true; // Interactions are registered only to guilds.

        var _request = request as CommandsCheckRequest;
        if (_request.Context == null) return null; // Command not depend on the context (Guild/DMs).

        // Command allows only DMs or only Guilds.
        return (_request.Context == Commands.ContextType.DM && request.Guild == null) ||
            (_request.Context == Commands.ContextType.Guild && request.Guild != null);
    }

    private async Task<bool?> CheckChannelPermissionsAsync(CheckRequestBase request)
    {
        if (request.ChannelPermissions == null) return null; // Command not depend on the channel permissions.

        var commandsRequest = request as CommandsCheckRequest;
        foreach (var perm in request.ChannelPermissions)
        {
            if (request is InteractionsCheckRequest interactionsRequest)
            {
                var _request = request as InteractionsCheckRequest;
                var attribute = new Interactions.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckRequirementsAsync(interactionsRequest.InteractionContext, _request.CommandInfo, ServiceProvider);

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
            if (request is InteractionsCheckRequest interactionsRequest)
            {
                var _request = request as InteractionsCheckRequest;
                var attribute = new Interactions.RequireUserPermissionAttribute(perm);
                var result = await attribute.CheckRequirementsAsync(interactionsRequest.InteractionContext, _request.CommandInfo, ServiceProvider);

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

        return request.User is SocketGuildUser user && user.Roles.Any(o => o.Tags?.IsPremiumSubscriberRole == true);
    }

    private static async Task<bool?> CheckExplicitAllowAsync(GrillBotContext dbContext, CheckRequestBase request)
    {
        var permissions = await dbContext.ExplicitPermissions.AsNoTracking()
            .Where(o => o.Command == request.CommandName.Trim() && o.State == ExplicitPermissionState.Allowed)
            .ToListAsync();

        if (permissions.Count == 0)
            return null;

        if (permissions.Any(o => !o.IsRole && o.TargetId == request.User.Id.ToString()))
            return true;

        return request.User is SocketGuildUser user && user.Roles.Any(role => permissions.Any(o => o.IsRole && o.TargetId == role.Id.ToString()));
    }

    private static async Task<bool> CheckExplicitBansAsync(GrillBotContext dbContext, CheckRequestBase request)
    {
        return await dbContext.ExplicitPermissions.AsNoTracking()
            .AnyAsync(o => o.Command == request.CommandName && !o.IsRole && o.State == ExplicitPermissionState.Banned && o.TargetId == request.User.Id.ToString());
    }
}
