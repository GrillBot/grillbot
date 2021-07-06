using Discord;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;

namespace GrillBot.App.Services.AuditLog
{
    public static class AuditLogTypeHelper
    {
        private static Dictionary<ActionType, AuditLogItemType> AuditLogItemTypes { get; } = new()
        {
            { ActionType.ChannelCreated, AuditLogItemType.ChannelCreated },
            { ActionType.ChannelDeleted, AuditLogItemType.ChannelDeleted },
            { ActionType.ChannelUpdated, AuditLogItemType.ChannelUpdated },
            { ActionType.EmojiDeleted, AuditLogItemType.EmojiDeleted },
            { ActionType.OverwriteCreated, AuditLogItemType.OverwriteCreated },
            { ActionType.OverwriteDeleted, AuditLogItemType.OverwriteDeleted },
            { ActionType.OverwriteUpdated, AuditLogItemType.OverwriteUpdated },
            { ActionType.Unban, AuditLogItemType.Unban },
            { ActionType.MemberUpdated, AuditLogItemType.MemberUpdated },
            { ActionType.MemberRoleUpdated, AuditLogItemType.MemberRoleUpdated },
            { ActionType.GuildUpdated, AuditLogItemType.GuildUpdated }
        };

        private static Dictionary<ActionType, Func<IAuditLogData, object>> MappingFunctions { get; } = new();

        public static bool IsDefined(ActionType type) => AuditLogItemTypes.ContainsKey(type) && MappingFunctions.ContainsKey(type);

        public static (AuditLogItemType type, object item) Convert(ActionType type, IAuditLogData data)
        {
            if (!IsDefined(type)) return (AuditLogItemType.None, null);

            return (
                AuditLogItemTypes[type],
                MappingFunctions[type](data)
            );
        }
    }
}
