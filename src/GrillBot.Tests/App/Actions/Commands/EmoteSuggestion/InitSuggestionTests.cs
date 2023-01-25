using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Discord;
using GrillBot.App.Actions.Commands.EmoteSuggestion;
using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using EmoteHelper = GrillBot.Tests.TestHelpers.EmoteHelper;

namespace GrillBot.Tests.App.Actions.Commands.EmoteSuggestion;

[TestClass]
public class InitSuggestionTests : CommandActionTest<InitSuggestion>
{
    private static readonly GuildEmote GuildEmote = EmoteHelper.CreateGuildEmote(Consts.OnlineEmote);

    protected override IGuild Guild
        => new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new[] { GuildEmote }).Build();

    protected override InitSuggestion CreateAction()
    {
        var message = new HttpResponseMessage();
        var httpClientFactory = HttpClientHelper.CreateFactory(message);
        var downloadHelper = new DownloadHelper(TestServices.CounterManager.Value, httpClientFactory);
        var cacheManager = new EmoteSuggestionManager(CacheBuilder);

        return InitAction(new InitSuggestion(TestServices.Texts.Value, downloadHelper, cacheManager));
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task ProcessAsync_ValidationFailed_NoData()
        => await Action.ProcessAsync(null, null);

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(ValidationException))]
    public async Task ProcessAsync_ValidationFailed_EmoteExists()
        => await Action.ProcessAsync(Consts.OnlineEmote, null);

    [TestMethod]
    public async Task ProcessAsync_Emote_Success()
        => await Action.ProcessAsync(Emote.Parse(Consts.PepeJamEmote), null);

    [TestMethod]
    public async Task ProcessAsync_Attachment_Success()
    {
        var attachment = new AttachmentBuilder().SetFilename("Filename.png").SetUrl("http://localhost").Build();
        await Action.ProcessAsync(null, attachment);
    }
}
