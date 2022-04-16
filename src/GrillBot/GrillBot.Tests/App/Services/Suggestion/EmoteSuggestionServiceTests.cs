using Discord;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.App.Services;
using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class EmoteSuggestionServiceTests : ServiceTest<EmoteSuggestionService>
{
    protected override EmoteSuggestionService CreateService()
    {
        var sesionService = new SuggestionSessionService();
        var discordClient = DiscordHelper.CreateClient();
        var guildService = new GuildService(discordClient, DbFactory);

        return new EmoteSuggestionService(sesionService, guildService, DbFactory);
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

        Service.InitSession(suggestionId, DataHelper.CreateEmote());
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ProcessSessionAsync_Attachment_Success()
    {
        var guild = DataHelper.CreateGuild();
        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = DataHelper.CreateChannel().Id.ToString();

        await DbContext.Guilds.AddAsync(guildEntity);
        await DbContext.SaveChangesAsync();

        var suggestionId = Guid.NewGuid().ToString();
        var user = DataHelper.CreateDiscordUser();

        var modalData = new EmoteSuggestionModal() { EmoteName = "Name" };

        Service.InitSession(suggestionId, DataHelper.CreateAttachment());
        await Service.ProcessSessionAsync(suggestionId, guild, user, modalData);
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(GrillBotException))]
    public async Task ProcessSessionAsync_NoBinaryData()
    {
        var guild = DataHelper.CreateGuild();
        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.EmoteSuggestionChannelId = DataHelper.CreateChannel().Id.ToString();

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
