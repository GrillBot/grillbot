using GrillBot.App.Services.FileStorage;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class FileStorageHelper
{
    public static FileStorageFactory Create(IConfiguration configuration)
    {
        var mock = new Mock<FileStorageFactory>(new object[] { configuration });
        mock.Setup(o => o.Create(It.IsAny<string>())).Returns(CreateStorage());

        return mock.Object;
    }

    public static IFileStorage CreateStorage()
    {
        var mock = new Mock<IFileStorage>();
        mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == "DeletedAttachments"), It.Is<string>(c => c == "Temp.txt"))).ReturnsAsync(new FileInfo("Temp.txt"));
        mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == "DeletedAttachments"), It.Is<string>(c => c == "Temporary.txt"))).ReturnsAsync(new FileInfo("Temporary.txt"));
        mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == "Clearing"), It.IsAny<string>())).ReturnsAsync(new FileInfo("File.xml"));

        return mock.Object;
    }
}
