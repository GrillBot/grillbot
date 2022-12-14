using Discord;
using Discord.Rest;
using GrillBot.App.Handlers.GuildUpdated;
using GrillBot.App.Services.AuditLog;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildUpdated;

[TestClass]
public class AuditEmotesGuildUpdatedHandlerTests : HandlerTest<AuditEmotesGuildUpdatedHandler>
{
    protected override AuditEmotesGuildUpdatedHandler CreateHandler()
    {
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        return new AuditEmotesGuildUpdatedHandler(TestServices.CounterManager.Value, auditLogWriter);
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        var emote = Emote.Parse(Consts.PepeJamEmote);
        var guildEmote = ReflectionHelper.CreateWithInternalConstructor<GuildEmote>(emote.Id, emote.Name, false, false, false, null, null);
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new List<GuildEmote> { guildEmote }).Build();
        await Handler.ProcessAsync(guild, guild);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        var emote = Emote.Parse(Consts.PepeJamEmote);
        var data = ReflectionHelper.CreateWithInternalConstructor<EmoteDeleteAuditLogData>(emote.Id, emote.Name);
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var entry = new AuditLogEntryBuilder(Consts.AuditLogEntryId).SetUser(user).SetActionType(ActionType.EmojiDeleted).SetData(data).Build();
        var guildEmote = ReflectionHelper.CreateWithInternalConstructor<GuildEmote>(emote.Id, emote.Name, false, false, false, null, null);
        var before = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new[] { guildEmote }).Build();
        var after = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new List<GuildEmote>()).SetGetAuditLogsAction(new[] { entry }).Build();

        await Handler.ProcessAsync(before, after);
    }
}
