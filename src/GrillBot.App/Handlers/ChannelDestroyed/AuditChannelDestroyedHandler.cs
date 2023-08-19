using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.ChannelDestroyed;

public class AuditChannelDestroyedHandler : AuditLogServiceHandler, IChannelDestroyedEvent
{
    public AuditChannelDestroyedHandler(IAuditLogServiceClient client) : base(client)
    {
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return;

        var request = CreateRequest(LogType.ChannelDeleted, guildChannel.Guild, guildChannel);
        request.ChannelInfo = new ChannelInfoRequest
        {
            Position = guildChannel.Position,
            Topic = (guildChannel as ITextChannel)?.Topic
        };

        await SendRequestAsync(request);
    }
}
