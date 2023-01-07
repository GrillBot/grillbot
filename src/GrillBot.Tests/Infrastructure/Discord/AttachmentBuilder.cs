using Discord;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Discord;

[ExcludeFromCodeCoverage]
public class AttachmentBuilder : BuilderBase<IAttachment>
{
    public AttachmentBuilder SetFilename(string filename)
    {
        Mock.Setup(o => o.Filename).Returns(filename);
        return this;
    }

    public AttachmentBuilder SetUrl(string url)
    {
        Mock.Setup(o => o.Url).Returns(url);
        Mock.Setup(o => o.ProxyUrl).Returns(url);
        return this;
    }
}
