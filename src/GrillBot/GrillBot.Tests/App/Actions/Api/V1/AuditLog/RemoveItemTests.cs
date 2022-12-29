using System.Diagnostics.CodeAnalysis;
using System.IO;
using Discord;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.AuditLog;

[TestClass]
public class RemoveItemTests : ApiActionTest<RemoveItem>
{
    protected override RemoveItem CreateAction()
    {
        var fileStorage = new FileStorageMock(TestServices.Configuration.Value);
        return new RemoveItem(ApiRequestContext, DatabaseBuilder, TestServices.Texts.Value, fileStorage);
    }

    [ExcludeFromCodeCoverage]
    protected override void Cleanup()
    {
        if (File.Exists("Temp.txt"))
            File.Delete("Temp.txt");
    }

    private async Task InitDataAsync(bool withFiles)
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var user = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        var logItem = new Database.Entity.AuditLogItem
        {
            Data = "{}",
            Guild = Database.Entity.Guild.FromDiscord(guild),
            Id = 1,
            Type = AuditLogItemType.MessageDeleted,
            CreatedAt = DateTime.Now,
            GuildChannel = Database.Entity.GuildChannel.FromDiscord(channel, ChannelType.Text),
            ProcessedUser = Database.Entity.User.FromDiscord(user)
        };

        if (withFiles)
        {
            logItem.Files.Add(new Database.Entity.AuditLogFileMeta
            {
                Filename = "Temp.txt",
                Id = 1,
                Size = 5
            });
        }

        await Repository.AddAsync(logItem);
        await Repository.CommitAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound() => await Action.ProcessAsync(1);

    [TestMethod]
    public async Task ProcessAsync_WithoutFiles()
    {
        const int id = 1;

        await InitDataAsync(false);
        await Action.ProcessAsync(id);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFiles_FileNotOnDisk()
    {
        await InitDataAsync(true);
        await Action.ProcessAsync(1);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFiles_FileOnDisk()
    {
        await InitDataAsync(true);
        await File.WriteAllBytesAsync("Temp.txt", new byte[] { 1, 2, 3, 4, 5 });

        await Action.ProcessAsync(1);
        Assert.IsFalse(File.Exists("Temp.txt"));
    }
}
