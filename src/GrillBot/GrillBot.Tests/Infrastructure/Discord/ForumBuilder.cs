using System.Linq;
using Discord;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

public class ForumBuilder : BuilderBase<IForumChannel>
{
    public ForumBuilder(ulong id, string name)
    {
        SetId(id);
        SetName(name);
    }

    public ForumBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public ForumBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public ForumBuilder SetPermissions(IEnumerable<Overwrite> overwrites)
    {
        var overwritesData = overwrites.ToList().AsReadOnly();
        Mock.Setup(o => o.PermissionOverwrites).Returns(overwritesData);

        foreach (var overwrite in overwritesData)
        {
            if (overwrite.TargetType == PermissionTarget.Role)
                Mock.Setup(o => o.GetPermissionOverwrite(It.Is<IRole>(r => r.Id == overwrite.TargetId))).Returns(overwrite.Permissions);
            else
                Mock.Setup(o => o.GetPermissionOverwrite(It.Is<IUser>(u => u.Id == overwrite.TargetId))).Returns(overwrite.Permissions);
        }

        return this;
    }

    public ForumBuilder SetTags(IEnumerable<ForumTag> tags)
    {
        Mock.Setup(o => o.Tags).Returns(tags.ToList().AsReadOnly());
        return this;
    }

    public ForumBuilder SetActiveThreadsAction(IEnumerable<IThreadChannel> threads)
    {
        var threadsData = threads.ToList().AsReadOnly();
        Mock.Setup(o => o.GetActiveThreadsAsync(It.IsAny<RequestOptions>())).ReturnsAsync(threadsData);
        return this;
    }

    public ForumBuilder SetTopic(string topic)
    {
        Mock.Setup(o => o.Topic).Returns(topic);
        return this;
    }
}
