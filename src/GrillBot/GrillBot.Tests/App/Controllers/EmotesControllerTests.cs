using GrillBot.App.Controllers;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using GrillBot.App.Services.AuditLog;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class EmotesControllerTests : ControllerTest<EmotesController>
{
    protected override EmotesController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var cacheService = new EmotesCacheService(discordClient);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new EmotesApiService(DatabaseBuilder, cacheService, ApiRequestContext, auditLogWriter);

        return new EmotesController(apiService, ServiceProvider);
    }

    [TestMethod]
    public async Task MergeStatsToAnotherAsync()
    {
        var @params = new MergeEmoteStatsParams
        {
            SourceEmoteId = Consts.FeelsHighManEmote,
            DestinationEmoteId = Consts.PepeJamEmote
        };

        var result = await Controller.MergeStatsToAnotherAsync(@params);
        CheckResult<BadRequestObjectResult, int>(result);
    }

    [TestMethod]
    public async Task RemoveStatisticsAsync_NoEmotes()
    {
        var result = await Controller.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }

    [TestMethod]
    public async Task RemoveStatisticsAsync_WithEmotes()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(new EmoteStatisticItem
        {
            EmoteId = Consts.PepeJamEmote,
            FirstOccurence = DateTime.MinValue,
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = GuildUser.FromDiscord(guild, guildUser),
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        var result = await Controller.RemoveStatisticsAsync(Consts.PepeJamEmote);
        CheckResult<OkObjectResult, int>(result);
    }

    [TestMethod]
    public async Task MergeStatsToAnotherAsync_Success()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        var guildEntity = Guild.FromDiscord(guild);
        var guildUserEntity = GuildUser.FromDiscord(guild, guildUser);

        await Repository.AddAsync(new EmoteStatisticItem
        {
            EmoteId = Consts.PepeJamEmote,
            FirstOccurence = DateTime.MinValue,
            Guild = guildEntity,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = guildUserEntity,
            UserId = Consts.UserId.ToString()
        });

        await Repository.AddAsync(new EmoteStatisticItem
        {
            EmoteId = Consts.FeelsHighManEmote,
            FirstOccurence = DateTime.MinValue,
            Guild = guildEntity,
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = guildUserEntity,
            UserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();

        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote,
            SuppressValidations = true
        };

        var result = await Controller.MergeStatsToAnotherAsync(mergeParams);
        CheckResult<OkObjectResult, int>(result);
    }

    [TestMethod]
    public async Task MergeStatsToAnotherAsync_NothingToProcess()
    {
        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote,
            SuppressValidations = true
        };

        var result = await Controller.MergeStatsToAnotherAsync(mergeParams);
        CheckResult<OkObjectResult, int>(result);
    }

    [TestMethod]
    public async Task MergeStatsToAnotherAsync_NothingInDestination()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(new EmoteStatisticItem
        {
            EmoteId = Consts.PepeJamEmote,
            FirstOccurence = new DateTime(2022, 6, 21, 23, 30, 45),
            Guild = Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = GuildUser.FromDiscord(guild, guildUser),
            UserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();

        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote,
            SuppressValidations = true
        };

        var result = await Controller.MergeStatsToAnotherAsync(mergeParams);
        CheckResult<OkObjectResult, int>(result);
    }
}
