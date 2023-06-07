using System.Diagnostics.CodeAnalysis;
using AuditLogService.Models.Request;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

namespace GrillBot.App.Handlers.ChannelUpdated;

public class AuditChannelUpdatedHandler : AuditLogServiceHandler, IChannelUpdatedEvent
{
    public AuditChannelUpdatedHandler(IAuditLogServiceClient auditLogServiceClient) : base(auditLogServiceClient)
    {
    }

    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        if (!Init(before, after, out var guildChannelBefore, out var guildChannelAfter)) return;

        var request = CreateRequest(LogType.ChannelUpdated, guildChannelAfter.Guild, guildChannelAfter);
        request.ChannelUpdated = new DiffRequest<ChannelInfoRequest>
        {
            After = CreateChannelInfoRequest(guildChannelAfter),
            Before = CreateChannelInfoRequest(guildChannelBefore)
        };

        await SendRequestAsync(request);
    }

    private static bool Init(IChannel before, IChannel after, [MaybeNullWhen(false)] out IGuildChannel guildChannelBefore, [MaybeNullWhen(false)] out IGuildChannel guildChannelAfter)
    {
        guildChannelBefore = before as IGuildChannel;
        guildChannelAfter = after as IGuildChannel;

        return guildChannelBefore is not null && guildChannelAfter is not null && !guildChannelBefore.IsEqual(guildChannelAfter);
    }

    private static ChannelInfoRequest CreateChannelInfoRequest(IGuildChannel channel)
    {
        return new ChannelInfoRequest
        {
            Position = channel.Position,
            Topic = (channel as ITextChannel)?.Topic
        };
    }
}
