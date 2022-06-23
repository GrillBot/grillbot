using Discord;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class ThreadBuilder : BuilderBase<IThreadChannel>
{
    public ThreadBuilder SetIdentity(ulong id, string name)
    {
        return SetId(id).SetName(name);
    }
    
    public ThreadBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public ThreadBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public ThreadBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        return this;
    }

    public ThreadBuilder SetType(ThreadType type)
    {
        Mock.Setup(o => o.Type).Returns(type);
        return this;
    }

    public ThreadBuilder IsArchived(bool isArchived = false)
    {
        Mock.Setup(o => o.IsArchived).Returns(isArchived);
        return this;
    }

    public ThreadBuilder SetSendFileAction(IUserMessage resultMessage, FileAttachment? attachment = null, string text = null)
    {
        Mock.Setup(o => o.SendFileAsync(
            attachment == null ? It.IsAny<FileAttachment>() : It.Is<FileAttachment>(x => x.FileName == attachment.Value.FileName && x.IsSpoiler == attachment.Value.IsSpoiler),
            string.IsNullOrEmpty(text) ? It.IsAny<string>() : It.Is<string>(x => x == text),
            It.IsAny<bool>(), It.IsAny<Embed>(), It.IsAny<RequestOptions>(), It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(), It.IsAny<MessageComponent>(),
            It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()
        )).Returns(Task.FromResult(resultMessage));
        return this;
    }

    public ThreadBuilder SetCategory(ulong categoryId)
    {
        Mock.Setup(o => o.CategoryId).Returns(categoryId);
        return this;
    }
}
