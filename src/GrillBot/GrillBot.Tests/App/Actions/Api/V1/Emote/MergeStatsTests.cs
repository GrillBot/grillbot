using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.App.Managers;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Emote;

[TestClass]
public class MergeStatsTests : ApiActionTest<MergeStats>
{
    private static readonly IGuild Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override MergeStats CreateAction()
    {
        var emotes = new[]
        {
            EmoteHelper.CreateGuildEmote(Discord.Emote.Parse(Consts.PepeJamEmote)),
            EmoteHelper.CreateGuildEmote(Discord.Emote.Parse(Consts.FeelsHighManEmote))
        };
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(emotes).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { guild }).Build();
        var emoteHelper = new GrillBot.App.Helpers.EmoteHelper(client);
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);

        return new MergeStats(ApiRequestContext, DatabaseBuilder, auditLogWriter, emoteHelper);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ValidateFails()
    {
        var parameters = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.OnlineEmoteId,
            SourceEmoteId = Consts.FeelsHighManEmote
        };

        await Action.ProcessAsync(parameters);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, guildUser));
        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.EmoteStatisticItem
            {
                EmoteId = Consts.PepeJamEmote,
                FirstOccurence = DateTime.MinValue,
                GuildId = Guild.Id.ToString(),
                LastOccurence = DateTime.MaxValue,
                UseCount = 1,
                UserId = Consts.UserId.ToString()
            },
            new Database.Entity.EmoteStatisticItem
            {
                EmoteId = Consts.FeelsHighManEmote,
                FirstOccurence = DateTime.MinValue,
                GuildId = Guild.Id.ToString(),
                LastOccurence = DateTime.MaxValue,
                UseCount = 1,
                UserId = Consts.UserId.ToString()
            }
        });

        await Repository.CommitAsync();

        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote
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
            SourceEmoteId = Consts.PepeJamEmote
        };

        var result = await Action.ProcessAsync(mergeParams);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task ProcessAsync_NothingInDestination()
    {
        var guildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(Guild).Build();

        await Repository.AddAsync(new Database.Entity.EmoteStatisticItem
        {
            EmoteId = Consts.PepeJamEmote,
            FirstOccurence = new DateTime(2022, 6, 21, 23, 30, 45),
            Guild = Database.Entity.Guild.FromDiscord(Guild),
            GuildId = Guild.Id.ToString(),
            LastOccurence = DateTime.MaxValue,
            UseCount = 1,
            User = Database.Entity.GuildUser.FromDiscord(Guild, guildUser),
            UserId = Consts.UserId.ToString()
        });

        await Repository.CommitAsync();

        var mergeParams = new MergeEmoteStatsParams
        {
            DestinationEmoteId = Consts.FeelsHighManEmote,
            SourceEmoteId = Consts.PepeJamEmote
        };

        var result = await Action.ProcessAsync(mergeParams);
        Assert.AreEqual(2, result);
    }
}
