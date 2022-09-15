using GrillBot.Common.Managers.Localization;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class TextsBuilder : BuilderBase<ITextsManager>
{
    public TextsBuilder AddText(string id, string locale, string text)
    {
        Mock.Setup(o => o[It.Is<string>(x => x == id), It.Is<string>(x => x == locale)]).Returns(text);
        return this;
    }
}
