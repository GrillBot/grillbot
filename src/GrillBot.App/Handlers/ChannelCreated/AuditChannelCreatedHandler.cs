using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.ChannelCreated;

public class AuditChannelCreatedHandler : AuditLogServiceHandler, IChannelCreatedEvent
{
    public AuditChannelCreatedHandler(IAuditLogServiceClient client) : base(client)
    {
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return;

        var request = CreateRequest(LogType.ChannelCreated, guildChannel.Guild, guildChannel);
        request.ChannelInfo = new ChannelInfoRequest
        {
            Position = guildChannel.Position,
            Topic = (guildChannel as ITextChannel)?.Topic
        };

        await SendRequestAsync(request);
    }
}
