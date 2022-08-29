using Discord;
using GrillBot.App.Infrastructure.IO;
using GrillBot.App.Services.Images;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class RendererFactoryMock : RendererFactory
{
    private TemporaryFile TemporaryFile { get; }

    public RendererFactoryMock(TemporaryFile temporaryFile, FileStorageMock fileStorageMock) : base(fileStorageMock, null)
    {
        TemporaryFile = temporaryFile;
    }

    public override RendererBase Create<TRenderer>()
    {
        var mock = new Mock<RendererBase>(FileStorage, null);

        mock.Setup(o => o.RenderAsync(It.IsAny<IUser>(), It.IsAny<IGuild>(), It.IsAny<IChannel>(), It.IsAny<IMessage>(), It.IsAny<IDiscordInteraction>()))
            .ReturnsAsync(TemporaryFile);

        return mock.Object;
    }
}
