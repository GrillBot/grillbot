using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class MergeStatsTests : ApiActionTest<MergeStats>
{
    protected override MergeStats CreateAction()
    {
        var discordClient = DiscordHelper.CreateClient();
        var cache = new EmotesCacheService(discordClient);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);

        return new MergeStats(ApiRequestContext, cache, DatabaseBuilder, auditLogWriter);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ValidateFails()
    {
        var parameters = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.PepeJamEmote,
            SourceEmoteId = Consts.FeelsHighManEmote
        };

        await Action.ProcessAsync(parameters);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(guild, guildUser));
        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.EmoteStatisticItem
            {
                EmoteId = Consts.PepeJamEmote,
                FirstOccurence = DateTime.MinValue,
                GuildId = guild.Id.ToString(),
                LastOccurence = DateTime.MaxValue,
                UseCount = 1,
                UserId = Consts.UserId.ToString()
            },
            new Database.Entity.EmoteStatisticItem
            {
                EmoteId = Consts.FeelsHighManEmote,
                FirstOccurence = DateTime.MinValue,
                GuildId = guild.Id.ToString(),
                LastOccurence = DateTime.MaxValue,
                UseCount = 1,
                UserId = Consts.UserId.ToString()
            }
        });

        await Repository.CommitAsync();

        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote,
            SuppressValidations = true
        };

        var result = await Action.ProcessAsync(mergeParams);
        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public async Task ProcessAsync_NothingToProcess()
    {
        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote,
            SuppressValidations = true
        };

        var result = await Action.ProcessAsync(mergeParams);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task ProcessAsync_NothingInDestination()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = Consts.PepeJamEmote,
            FirstOccurence = new DateTime(2022, 6, 21, 23, 30, 45),
            Guild = Database.Entity.Guild.FromDiscord(guild),
            GuildId = guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = Database.Entity.GuildUser.FromDiscord(guild, guildUser),
            UserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();

        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote,
            SuppressValidations = true
        };

        var result = await Action.ProcessAsync(mergeParams);
        Assert.AreEqual(2, result);
    }
}
