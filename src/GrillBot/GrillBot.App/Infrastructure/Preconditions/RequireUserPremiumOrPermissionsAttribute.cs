using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.Preconditions
{
    public class RequireUserPremiumOrPermissionsAttribute : PreconditionAttribute
    {
        public List<GuildPermission> Permissions { get; }

        public RequireUserPremiumOrPermissionsAttribute(params GuildPermission[] permissions)
        {
            Permissions = permissions.ToList();
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is not SocketGuildUser user)
                return PreconditionResult.FromError("Invalid user type. Required guild user.");

            if (user.Roles.Any(o => !o.IsEveryone && o.Tags?.IsPremiumSubscriberRole == true))
                return PreconditionResult.FromSuccess();

            foreach (var permission in Permissions)
            {
                var attribute = new RequireUserPermissionAttribute(permission);

                if ((await attribute.CheckPermissionsAsync(context, command, services)).IsSuccess)
                    return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError(ErrorMessage);
        }
    }
}
