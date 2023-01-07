using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Common;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class EmojizationTests : CommandActionTest<Emojization>
{
    protected override IDiscordInteraction Interaction { get; }
        = new DiscordInteractionBuilder(Consts.InteractionId).Build();

    protected override IGuild Guild { get; }
        = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetEmotes(new[] { EmoteHelper.CreateGuildEmote(Emote.Parse(Consts.PepeJamEmote)) }).Build();

    protected override Emojization CreateAction()
    {
        return InitAction(new Emojization(TestServices.Texts.Value));
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public void Process_NoContent() => Action.Process(null);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public void ProcessForReacts_DuplicateCharacter() => Action.ProcessForReacts("TT", 5);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public void ProcessForReacts_DuplicateEmote() => Action.ProcessForReacts($"{Consts.PepeJamEmote} {Consts.PepeJamEmote}", 5);

    [TestMethod]
    public void Process_Success()
    {
        var msg = $"{Consts.PepeJamEmote}  PEPEJAM /()/ {Emojis.LetterA} <ABCD> {Consts.PepeJamEmote}";
        var result = Action.Process(msg);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(Consts.PepeJamEmote));
    }

    [TestMethod]
    public void ProcessForReacts_Success()
    {
        const string msg = "AABBOOEEPPIIXX";
        var result = Action.ProcessForReacts(msg, int.MaxValue).ToList();

        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public void Process_OnlyInvalidCharacters() => Action.Process("@&");

    [TestMethod]
    public void Process_WithExternalEmote()
    {
        const string msg = $"{Consts.PepeJamEmote} {Consts.OnlineEmoteId} TEST";
        var result = Action.Process(msg);

        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.IsTrue(result.Contains(' '));
        Assert.IsTrue(result.Contains(Consts.PepeJamEmote));
        Assert.IsFalse(result.Contains(Consts.OnlineEmoteId));
    }
}
