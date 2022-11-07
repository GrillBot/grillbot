using Discord;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class TextChannelBuilder : BuilderBase<ITextChannel>
{
    public TextChannelBuilder(ulong id, string name)
    {
        Mock.Setup(o => o.DeleteMessagesAsync(It.IsAny<IEnumerable<IMessage>>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask);

        SetId(id);
        SetName(name);
    }

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

    public TextChannelBuilder SetSendFilesAction(IUserMessage message)
    {
        Mock.Setup(o => o.SendFilesAsync(It.IsAny<IEnumerable<FileAttachment>>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(), It.IsAny<RequestOptions>(),
                It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(), It.IsAny<MessageComponent>(), It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>()))
            .Returns(Task.FromResult(message));
        return this;
    }

    public TextChannelBuilder SetGetMessageAsync(IMessage message)
    {
        Mock.Setup(o => o.GetMessageAsync(It.Is<ulong>(x => x == message.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(message));
        return this;
    }

    public TextChannelBuilder SetGetMessagesAsync(IEnumerable<IMessage> message)
    {
        var messages = message.ToList().AsReadOnly();
        var data = new List<IReadOnlyCollection<IMessage>> { messages }.ToAsyncEnumerable();

        Mock.Setup(o => o.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(data);
        return this;
    }

    public TextChannelBuilder SetSendMessageAction(IUserMessage message)
    {
        Mock.Setup(o => o.SendMessageAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Embed>(), It.IsAny<RequestOptions>(), It.IsAny<AllowedMentions>(), It.IsAny<MessageReference>(),
            It.IsAny<MessageComponent>(), It.IsAny<ISticker[]>(), It.IsAny<Embed[]>(), It.IsAny<MessageFlags>())).ReturnsAsync(message);
        return this;
    }

    public TextChannelBuilder SetGetUsersAction(IEnumerable<IGuildUser> users)
    {
        var usersData = users.ToList();
        var enumerable = new List<IReadOnlyCollection<IGuildUser>> { usersData.AsReadOnly() }.ToAsyncEnumerable();
        Mock.Setup(o => o.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(enumerable);

        foreach (var user in usersData)
            Mock.Setup(o => o.GetUserAsync(It.Is<ulong>(x => x == user.Id), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(user);
        return this;
    }

    public TextChannelBuilder SetGetPinnedMessagesAction(IEnumerable<IMessage> messages)
    {
        var data = messages.ToList().AsReadOnly();
        Mock.Setup(o => o.GetPinnedMessagesAsync(It.IsAny<RequestOptions>())).ReturnsAsync(data);
        return this;
    }

    public TextChannelBuilder SetPermissions(IEnumerable<Overwrite> overwrites)
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

    public TextChannelBuilder SetCategory(ICategoryChannel category)
    {
        Mock.Setup(o => o.CategoryId).Returns(category.Id);
        Mock.Setup(o => o.GetCategoryAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).ReturnsAsync(category);
        return this;
    }
}
