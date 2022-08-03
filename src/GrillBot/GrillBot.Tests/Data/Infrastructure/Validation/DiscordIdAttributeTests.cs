using GrillBot.Data.Infrastructure.Validation;

namespace GrillBot.Tests.Data.Infrastructure.Validation;

[TestClass]
public class DiscordIdAttributeTests : ValidationAttributeTest<DiscordIdAttribute>
{
    protected override DiscordIdAttribute CreateAttribute() => new();

    [TestMethod]
    public void IsValid_UnsupportedType()
    {
        var result = Execute(new List<int>());
        CheckFail(result, DiscordIdAttribute.UnsupportedType);
    }

    [TestMethod]
    public void IsValid_Null()
    {
        var result = Execute(null);
        CheckSuccess(result);
    }

    [TestMethod]
    public void IsValid_Ulong_Zero()
    {
        var result = Execute((ulong)0);
        CheckFail(result, DiscordIdAttribute.InvalidFormat);
    }

    [TestMethod]
    public void IsValid_Ulong_InvalidValue()
    {
        var result = Execute((ulong)42);
        CheckFail(result, DiscordIdAttribute.InvalidFormat);
    }

    [TestMethod]
    public void IsValid_String_InvalidStringFormat()
    {
        var result = Execute(Consts.Nickname);
        CheckFail(result, DiscordIdAttribute.InvalidStringFormat);
    }

    [TestMethod]
    public void IsValid_StringCollection()
    {
        var data = new List<string> { Consts.ChannelId.ToString() };
        var result = Execute(data);

        CheckSuccess(result);
    }

    [TestMethod]
    public void IsValid_StringCollection_InvalidValue()
    {
        var data = new List<string> { "0", "", " " };
        var result = Execute(data);

        CheckFail(result, DiscordIdAttribute.InvalidFormat);
    }

    [TestMethod]
    public void IsValid_UlongCollection()
    {
        var data = new List<ulong> { Consts.ChannelId };
        var result = Execute(data);

        CheckSuccess(result);
    }
}
