using Discord.Interactions;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Permissions.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireUserPermsAttribute : PreconditionAttribute
{
    public GuildPermission[] GuildPermissions { get; set; }
    public ChannelPermission[] ChannelPermissions { get; set; }
    public bool AllowBooster { get; set; }

    public RequireUserPermsAttribute() { }

    public RequireUserPermsAttribute(GuildPermission[] guildPermissions)
    {
        GuildPermissions = guildPermissions;
    }

    public RequireUserPermsAttribute(ChannelPermission[] channelPermissions)
    {
        ChannelPermissions = channelPermissions;
    }

    public RequireUserPermsAttribute(GuildPermission permission) : this(new[] { permission }) { }
    public RequireUserPermsAttribute(ChannelPermission permission) : this(new[] { permission }) { }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var service = services.GetRequiredService<PermissionsService>();
        var request = new InteractionsCheckRequest()
        {
            AllowBooster = AllowBooster,
            ChannelPermissions = ChannelPermissions,
            GuildPermissions = GuildPermissions,
            CommandInfo = commandInfo,
            InteractionContext = context
        };

        var result = await service.CheckPermissionsAsync(request);

        if (result.IsAllowed())
            return PreconditionResult.FromSuccess();

        return PreconditionResult.FromError(result.ToString());
    }
}
