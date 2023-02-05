using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Common.Services.Math.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class SolveExpressionTests : CommandActionTest<SolveExpression>
{
    protected override IGuildUser User
        => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    protected override SolveExpression CreateAction()
    {
        var client = new MathClientBuilder()
            .SetSolveExpressionAction("a+a", new MathJsResult { Error = "Undefined variable: a" })
            .SetSolveExpressionAction("1+1", new MathJsResult { Result = "2" })
            .SetSolveExpressionAction("11111 * 22222", new MathJsResult { IsTimeout = true })
            .Build();

        return InitAction(new SolveExpression(client, TestServices.Texts.Value));
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var result = await Action.ProcessAsync("1+1");
        CheckEmbed(result, Color.Green);
    }

    [TestMethod]
    public async Task ProcessAsync_Timeout()
    {
        var result = await Action.ProcessAsync("11111 * 22222");
        CheckEmbed(result, Color.Red);
    }

    [TestMethod]
    public async Task ProcessAsync_Error()
    {
        var result = await Action.ProcessAsync("a+a");
        CheckEmbed(result, Color.Red);
    }

    private static void CheckEmbed(IEmbed? embed, Color color)
    {
        Assert.IsNotNull(embed);
        Assert.IsNotNull(embed.Color);
        Assert.AreEqual(color, embed.Color);
    }
}
