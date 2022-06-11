using System.IO;
using GrillBot.Common.FileStorage;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class FileStorageMock : FileStorageFactory
{
    public FileStorageMock(IConfiguration configuration) : base(configuration)
    {
    }

    public override IFileStorage Create(string categoryName)
    {
        var mock = new Mock<IFileStorage>();
        mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == "DeletedAttachments"), It.Is<string>(c => c == "Temp.txt"))).ReturnsAsync(new FileInfo("Temp.txt"));
        mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == "DeletedAttachments"), It.Is<string>(c => c == "Temporary.txt"))).ReturnsAsync(new FileInfo("Temporary.txt"));
        mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == "Clearing"), It.IsAny<string>())).ReturnsAsync(new FileInfo("File.xml"));

        return mock.Object;
    }
}
