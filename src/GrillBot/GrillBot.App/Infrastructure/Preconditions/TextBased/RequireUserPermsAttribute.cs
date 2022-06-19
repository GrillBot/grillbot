using Discord.Commands;
using GrillBot.App.Services.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.TextBased;

public class RequireUserPermsAttribute : PreconditionAttribute
{
    public ContextType? Contexts { get; set; }
    public GuildPermission[] GuildPermissions { get; set; }
    public ChannelPermission[] ChannelPermissions { get; set; }
    public bool AllowBooster { get; set; }

    public RequireUserPermsAttribute()
    {
    }

    public RequireUserPermsAttribute(ContextType context)
    {
        Contexts = context;
    }

    private RequireUserPermsAttribute(GuildPermission[] guildPermissions)
    {
        GuildPermissions = guildPermissions;
        Contexts = ContextType.Guild;
    }

    private RequireUserPermsAttribute(ChannelPermission[] channelPermissions)
    {
        ChannelPermissions = channelPermissions;
    }

    public RequireUserPermsAttribute(GuildPermission permission) : this(new[] { permission })
    {
    }

    public RequireUserPermsAttribute(ChannelPermission permission) : this(new[] { permission })
    {
    }

    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var service = services.GetRequiredService<PermissionsService>();
        var request = new CommandsCheckRequest
        {
            AllowBooster = AllowBooster,
            ChannelPermissions = ChannelPermissions,
            CommandContext = context,
            CommandInfo = command,
            Context = Contexts,
            GuildPermissions = GuildPermissions
        };

        var result = await service.CheckPermissionsAsync(request);
        return result.IsAllowed() ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(result.ToString());
    }
}
