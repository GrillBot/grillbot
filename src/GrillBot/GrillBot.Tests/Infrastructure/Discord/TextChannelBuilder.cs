using Discord;
using System.Diagnostics.CodeAnalysis;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class TextChannelBuilder : BuilderBase<ITextChannel>
{
    public TextChannelBuilder SetIdentity(ulong id, string name)
        => SetId(id).SetName(name);

    public TextChannelBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        Mock.Setup(o => o.CreatedAt).Returns(SnowflakeUtils.FromSnowflake(id));
        return this;
    }

    public TextChannelBuilder SetName(string name)
    {
        Mock.Setup(o => o.Name).Returns(name);
        return this;
    }

    public TextChannelBuilder SetNsfw(bool isNsfw)
    {
        Mock.Setup(o => o.IsNsfw).Returns(isNsfw);
        return this;
    }

    public TextChannelBuilder SetGuild(IGuild guild)
    {
        Mock.Setup(o => o.Guild).Returns(guild);
        Mock.Setup(o => o.GuildId).Returns(guild.Id);
        return this;
    }

    public TextChannelBuilder SetSendFileAction(string filename, IUserMessage message)
    {
        Mock.Setup(o => o.SendFileAsync(It.Is<FileAttachment>(x => x.FileName == filename), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(), It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(), It.IsAny<MessageComponent>(), It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()))
            .Returns(Task.FromResult(message));
        return this;
    }

    public TextChannelBuilder SetGetMessageAsync(IMessage message)
    {
        Mock.Setup(o => o.GetMessageAsync(It.Is<ulong>(o => o == message.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(message));
        return this;
    }
}
