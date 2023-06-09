using GrillBot.Common.Extensions;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Common.Services.AuditLog.Models.Request.Search;

public class AdvancedSearchRequest : IDictionaryObject
{
    public TextSearchRequest? Info { get; set; }
    public TextSearchRequest? Warning { get; set; }
    public TextSearchRequest? Error { get; set; }
    public ExecutionSearchRequest? Interaction { get; set; }
    public ExecutionSearchRequest? Job { get; set; }
    public ApiSearchRequest? Api { get; set; }
    public UserIdSearchRequest? OverwriteCreated { get; set; }
    public UserIdSearchRequest? OverwriteDeleted { get; set; }
    public UserIdSearchRequest? OverwriteUpdated { get; set; }
    public UserIdSearchRequest? MemberRolesUpdated { get; set; }
    public UserIdSearchRequest? MemberUpdated { get; set; }
    public MessageDeletedSearchRequest? MessageDeleted { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>();

        result.MergeDictionaryObjects(Info, nameof(Info));
        result.MergeDictionaryObjects(Warning, nameof(Warning));
        result.MergeDictionaryObjects(Error, nameof(Error));
        result.MergeDictionaryObjects(Interaction, nameof(Interaction));
        result.MergeDictionaryObjects(Job, nameof(Job));
        result.MergeDictionaryObjects(Api, nameof(Api));
        result.MergeDictionaryObjects(OverwriteCreated, nameof(OverwriteCreated));
        result.MergeDictionaryObjects(OverwriteDeleted, nameof(OverwriteDeleted));
        result.MergeDictionaryObjects(OverwriteUpdated, nameof(OverwriteUpdated));
        result.MergeDictionaryObjects(MemberRolesUpdated, nameof(MemberRolesUpdated));
        result.MergeDictionaryObjects(MemberUpdated, nameof(MemberUpdated));
        result.MergeDictionaryObjects(MessageDeleted, nameof(MessageDeleted));

        return result;
    }
}
