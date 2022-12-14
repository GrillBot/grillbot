using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.App.Managers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Channel;

[TestClass]
public class UpdateChannelTests : ApiActionTest<UpdateChannel>
{
    private IGuild Guild { get; set; }
    private ITextChannel TextChannel { get; set; }

    protected override UpdateChannel CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        TextChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetTextChannelAction(TextChannel).Build();

        var discordClient = DiscordHelper.CreateClient();
        var manager = new AutoReplyManager(DatabaseBuilder);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var texts = new TextsBuilder()
            .AddText("ChannelModule/ChannelDetail/ChannelNotFound", "cs", "ChannelNotFound")
            .Build();
        var auditLogService = new AuditLogService(discordClient, DatabaseBuilder);

        return new UpdateChannel(ApiRequestContext, DatabaseBuilder, auditLogWriter, texts, auditLogService, manager);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ChannelNotFound()
        => await Action.ProcessAsync(Consts.ChannelId, new UpdateChannelParams());

    [TestMethod]
    public async Task ProcessAsync_NoChanged()
    {
        await InitChannelAsync();
        await Action.ProcessAsync(Consts.ChannelId, new UpdateChannelParams());
    }

    [TestMethod]
    public async Task ProcessAsync_ReloadAutoReply()
    {
        await InitChannelAsync();
        await Action.ProcessAsync(Consts.ChannelId, new UpdateChannelParams { Flags = (long)ChannelFlags.AutoReplyDeactivated });
    }

    private async Task InitChannelAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildChannel.FromDiscord(TextChannel, ChannelType.Text));
        await Repository.CommitAsync();
    }
}
