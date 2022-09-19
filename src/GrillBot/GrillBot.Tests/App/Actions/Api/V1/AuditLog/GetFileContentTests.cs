using System.IO;
using Discord;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.AuditLog;

[TestClass]
public class GetFileContentTests : ApiActionTest<GetFileContent>
{
    protected override GetFileContent CreateAction()
    {
        var fileStorage = new FileStorageMock(TestServices.Configuration.Value);
        var texts = new TextsBuilder()
            .AddText("AuditLog/GetFileContent/NotFound", "cs", "NotFound")
            .Build();

        return new GetFileContent(ApiRequestContext, DatabaseBuilder, fileStorage, texts);
    }

    protected override void Cleanup()
    {
        if (File.Exists("Temp.txt")) File.Delete("Temp.txt");
        if (File.Exists("Temp.unknown")) File.Delete("Temp.unknown");
    }

    [TestMethod]
    public async Task ProcessAsync_ItemNotFound()
    {
        var result = await Action.ProcessAsync(1, 1);

        Assert.IsNull(result.content);
        Assert.IsTrue(string.IsNullOrEmpty(result.contentType));
        Assert.IsFalse(string.IsNullOrEmpty(result.errMsg));
    }

    [TestMethod]
    public async Task ProcessAsync_MetadataNotFound()
    {
        await InitDataAsync(false, null);

        var result = await Action.ProcessAsync(1, 1);

        Assert.IsNull(result.content);
        Assert.IsTrue(string.IsNullOrEmpty(result.contentType));
        Assert.IsFalse(string.IsNullOrEmpty(result.errMsg));
    }

    [TestMethod]
    public async Task ProcessAsync_FileNotExists()
    {
        await InitDataAsync(true, "txt");

        var result = await Action.ProcessAsync(1, 1);

        Assert.IsNull(result.content);
        Assert.IsTrue(string.IsNullOrEmpty(result.contentType));
        Assert.IsFalse(string.IsNullOrEmpty(result.errMsg));
    }

    [TestMethod]
    public async Task ProcessAsync_UnknownContentType()
    {
        await InitDataAsync(true, "unknown");
        await File.WriteAllBytesAsync("Temp.unknown", new byte[] { 1, 2, 3, 4, 5 });

        var result = await Action.ProcessAsync(1, 1);

        Assert.IsNotNull(result.content);
        Assert.AreEqual(5, result.content.Length);
        Assert.AreEqual("application/octet-stream", result.contentType);
        Assert.IsTrue(string.IsNullOrEmpty(result.errMsg));
    }

    [TestMethod]
    public async Task ProcessAsync_KnownContentType()
    {
        await InitDataAsync(true, "txt");
        await File.WriteAllBytesAsync("Temp.txt", new byte[] { 1, 2, 3, 4, 5 });

        var result = await Action.ProcessAsync(1, 1);

        Assert.IsNotNull(result.content);
        Assert.AreEqual(5, result.content.Length);
        Assert.AreEqual("text/plain", result.contentType);
        Assert.IsTrue(string.IsNullOrEmpty(result.errMsg));
    }

    private async Task InitDataAsync(bool withFiles, string extension)
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var item = new AuditLogItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.MessageDeleted,
            Id = 1
        };

        if (withFiles)
        {
            item.Files.Add(new AuditLogFileMeta
            {
                Filename = $"Temp.{extension}",
                Size = 5,
                Id = 1
            });
        }

        await Repository.AddAsync(item);
        await Repository.AddAsync(Guild.FromDiscord(guild));
        await Repository.AddAsync(GuildChannel.FromDiscord(channel, ChannelType.Text));
        await Repository.AddAsync(User.FromDiscord(user));
        await Repository.CommitAsync();
    }
}
