using Discord;

namespace GrillBot.Tests.Infrastructure.Discord;

public class AttachmentBuilder : BuilderBase<IAttachment>
{
    public AttachmentBuilder SetId(ulong id)
    {
        Mock.Setup(o => o.Id).Returns(id);
        return this;
    }

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
