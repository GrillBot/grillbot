using GrillBot.Data.Infrastructure.Validation;

namespace GrillBot.Tests.Data.Infrastructure.Validation;

[TestClass]
public class EmoteIdAttributeTests : ValidationAttributeTest<EmoteIdAttribute>
{
    protected override EmoteIdAttribute CreateAttribute() => new();

    [TestMethod]
    public void IsValid_NotString()
    {
        var result = Execute(0);
        CheckSuccess(result);
    }

    [TestMethod]
    public void IsValid_NotEmote()
    {
        var result = Execute("HelloWorld");

        CheckFail(result, Attribute.ErrorMessage);
    }

    [TestMethod]
    public void IsValid_Success()
    {
        var result = Execute(Consts.OnlineEmoteId);
        CheckSuccess(result);
    }
}
