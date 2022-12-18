using System.Threading.Channels;
using Discord;
using Discord.Rest;
using GrillBot.App.Handlers.ChannelUpdated;
using GrillBot.App.Managers;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.ChannelUpdated;

[TestClass]
public class AuditOverwritesChangedHandlerTests : HandlerTest<AuditOverwritesChangedHandler>
{
    private ITextChannel TextChannel { get; set; }
    private AuditLogManager AuditLogManager { get; set; }

    protected override AuditOverwritesChangedHandler CreateHandler()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        AuditLogManager = new AuditLogManager();
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);

        return new AuditOverwritesChangedHandler(AuditLogManager, DatabaseBuilder, TestServices.CounterManager.Value, auditLogWriter);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(TextChannel.Guild));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));

        await Repository.AddAsync(new Database.Entity.AuditLogItem
        {
            GuildId = Consts.GuildId.ToString(),
            ChannelId = Consts.ChannelId.ToString(),
            Type = AuditLogItemType.OverwriteCreated,
            CreatedAt = DateTime.Now,
            DiscordAuditLogItemId = (Consts.AuditLogEntryId + 1).ToString(),
            Data = "{}"
        });

        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Dms()
    {
        var channel = new DmChannelBuilder().Build();
        await Handler.ProcessAsync(null, channel);
    }

    [TestMethod]
    public async Task ProcessAsync_TimeLimit()
    {
        AuditLogManager.OnOverwriteChangedEvent(TextChannel.Id, DateTime.Now.AddMonths(1));
        await Handler.ProcessAsync(TextChannel, TextChannel);
    }

    [TestMethod]
    public async Task ProcessAsync_NoAuditLog()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(new List<IAuditLogEntry>()).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Handler.ProcessAsync(channel, channel);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var entries = new List<IAuditLogEntry>
        {
            new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetActionType(ActionType.OverwriteCreated).SetUser(user)
                .SetData(ReflectionHelper.CreateWithInternalConstructor<OverwriteCreateAuditLogData>(Consts.ChannelId, new Overwrite())).Build(),
            new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetActionType(ActionType.OverwriteDeleted).SetUser(user)
                .SetData(ReflectionHelper.CreateWithInternalConstructor<OverwriteDeleteAuditLogData>(Consts.ChannelId, new Overwrite())).Build(),
            new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetActionType(ActionType.OverwriteUpdated).SetUser(user)
                .SetData(ReflectionHelper.CreateWithInternalConstructor<OverwriteUpdateAuditLogData>(Consts.ChannelId, new OverwritePermissions(),
                    new OverwritePermissions(), 0UL, PermissionTarget.Role)).Build(),
            new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetActionType(ActionType.Ban).SetUser(user).SetData(null).Build(),
            new AuditLogEntryBuilder(Consts.AuditLogEntryId + 1).SetActionType(ActionType.Ban).SetUser(user).SetData(null).Build()
        };
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetAuditLogsAction(entries).Build();
        var textChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();

        await Handler.ProcessAsync(textChannel, textChannel);
    }
}
