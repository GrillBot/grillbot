using AuditLogService.Models.Request;
using GrillBot.Common.Services.AuditLog.Enums;

namespace GrillBot.Common.Services.AuditLog.Models;

public class LogRequest
{
    public DateTime? CreatedAt { get; set; }
    public string? GuildId { get; set; }
    public string? UserId { get; set; }
    public string? ChannelId { get; set; }
    public string? DiscordId { get; set; }
    public LogType Type { get; set; }
    public List<FileRequest> Files { get; set; } = new();
    public ApiRequestRequest? ApiRequest { get; set; }
    public LogMessageRequest? LogMessage { get; set; }
    public DeletedEmoteRequest? DeletedEmote { get; set; }
    public UnbanRequest? Unban { get; set; }
    public JobExecutionRequest? JobExecution { get; set; }
    public ChannelInfoRequest? ChannelInfo { get; set; }
    public DiffRequest<ChannelInfoRequest>? ChannelUpdated { get; set; }
    public DiffRequest<GuildInfoRequest>? GuildUpdated { get; set; }
    public MessageDeletedRequest? MessageDeleted { get; set; }
    public MessageEditedRequest? MessageEdited { get; set; }
    public UserJoinedRequest? UserJoined { get; set; }
    public UserLeftRequest? UserLeft { get; set; }
    public InteractionCommandRequest? InteractionCommand { get; set; }
    public ThreadInfoRequest? ThreadInfo { get; set; }
    public DiffRequest<ThreadInfoRequest>? ThreadUpdated { get; set; }
    public MemberUpdatedRequest? MemberUpdated { get; set; }
}
