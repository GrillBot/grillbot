using Discord;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class EmoteSuggestionServiceTests : ServiceTest<EmoteSuggestionService>
{
    protected override EmoteSuggestionService CreateService()
    {
        var sesionService = new SuggestionSessionService();

        return new EmoteSuggestionService(sesionService, DbFactory);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessSessionAsync_NoMetadata()
    {
        var suggestionId = Guid.NewGuid().ToString();
        var guild = DataHelper.CreateGuild();
        var user = DataHelper.CreateDiscordUser();
        var modalData = new EmoteSuggestionModal();

        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task ProcessSessionAsync_WithDescription_InvalidChannel()
    {
        var guild = DataHelper.CreateGuild();
        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = "123456789";

        await DbContext.Guilds.AddAsync(guildEntity);
        await DbContext.SaveChangesAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = DataHelper.CreateDiscordUser();

        var modalData = new EmoteSuggestionModal()
        {
            EmoteDescription = "Popis",
            EmoteName = "Name"
        };

        Service.InitSession(suggestionId, Emote.Parse(Consts.OnlineEmoteId));
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ProcessSessionAsync_Attachment_Success()
    {
        var channel = new TextChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName)
            .Build();

        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .SetGetTextChannelAction(channel)
            .SetRoles(Enumerable.Empty<IRole>())
            .Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await DbContext.Guilds.AddAsync(guildEntity);
        await DbContext.SaveChangesAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = DataHelper.CreateDiscordUser();
        var attachment = new AttachmentBuilder()
            .SetFilename("File.png")
            .SetUrl("https://www.google.com/images/searchbox/desktop_searchbox_sprites318_hr.png")
            .Build();

        var modalData = new EmoteSuggestionModal() { EmoteName = "Name" };

        Service.InitSession(suggestionId, attachment);
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(GrillBotException))]
    public async Task ProcessSessionAsync_NoBinaryData()
    {
        var channel = new TextChannelBuilder()
            .SetId(Consts.ChannelId).SetName(Consts.ChannelName)
            .Build();

        var guild = new GuildBuilder()
            .SetId(Consts.GuildId).SetName(Consts.GuildName)
            .SetGetTextChannelAction(channel)
            .SetRoles(Enumerable.Empty<IRole>())
            .Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = Consts.ChannelId.ToString();

        await DbContext.Guilds.AddAsync(guildEntity);
        await DbContext.SaveChangesAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = DataHelper.CreateDiscordUser();

        var modalData = new EmoteSuggestionModal() { EmoteName = "Name" };

        Service.InitSession(suggestionId, null);
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }
}
