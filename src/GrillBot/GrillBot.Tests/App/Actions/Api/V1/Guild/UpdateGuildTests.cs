using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Guild;

[TestClass]
public class UpdateGuildTests : ApiActionTest<UpdateGuild>
{
    private IGuild Guild { get; set; }

    protected override UpdateGuild CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        var textChannel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guildBuilder.Build()).Build();
        var role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        Guild = guildBuilder.SetGetChannelsAction(new[] { textChannel }).SetRoles(new[] { role }).SetGetUsersAction(Enumerable.Empty<IGuildUser>()).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();
        var texts = TestServices.Texts.Value;
        var getGuildDetail = new GetGuildDetail(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, client, CacheBuilder, texts);
        return new UpdateGuild(ApiRequestContext, client, DatabaseBuilder, getGuildDetail, texts);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound()
    {
        var parameters = new UpdateGuildParams();
        await Action.ProcessAsync(Consts.GuildId + 1, parameters);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var parameters = new UpdateGuildParams
        {
            AdminChannelId = Consts.ChannelId.ToString(),
            MuteRoleId = Consts.RoleId.ToString(),
            VoteChannelId = Consts.ChannelId.ToString(),
            EmoteSuggestionChannelId = Consts.ChannelId.ToString(),
            EmoteSuggestionsValidity = new RangeParams<DateTime> { From = DateTime.MinValue, To = DateTime.MaxValue },
            BotRoomChannelId = Consts.ChannelId.ToString()
        };

        var result = await Action.ProcessAsync(Consts.GuildId, parameters);
        GetGuildDetailTests.CheckSuccess(result, false);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_ValidationErrors()
    {
        var cases = new[]
        {
            new UpdateGuildParams { AdminChannelId = (Consts.ChannelId + 1).ToString() },
            new UpdateGuildParams { MuteRoleId = (Consts.RoleId + 1).ToString() },
            new UpdateGuildParams { EmoteSuggestionChannelId = (Consts.ChannelId + 1).ToString() },
            new UpdateGuildParams { VoteChannelId = (Consts.ChannelId + 1).ToString() },
            new UpdateGuildParams { BotRoomChannelId = (Consts.ChannelId + 1).ToString() }
        };

        foreach (var @case in cases)
        {
            try
            {
                await Action.ProcessAsync(Consts.GuildId, @case);
                Assert.Fail("ProcessAsync not thrown exception");
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vex && !string.IsNullOrEmpty(vex.ValidationResult.ErrorMessage))
                    continue;

                throw;
            }
        }
    }
}
