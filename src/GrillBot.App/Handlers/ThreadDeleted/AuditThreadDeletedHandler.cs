using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.ThreadDeleted;

public class AuditThreadDeletedHandler : AuditLogServiceHandler, IThreadDeletedEvent
{
    private ChannelHelper ChannelHelper { get; }

    public AuditThreadDeletedHandler(ChannelHelper channelHelper, IAuditLogServiceClient client) : base(client)
    {
        ChannelHelper = channelHelper;
    }

    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        var guild = await ChannelHelper.GetGuildFromChannelAsync(cachedThread, threadId);
        if (guild is null) return;

        var request = CreateRequest(LogType.ThreadDeleted, guild);
        request.ChannelId = threadId.ToString();
        request.ThreadInfo = new ThreadInfoRequest { Tags = new List<string>() };

        await SendRequestAsync(request);
    }
}
