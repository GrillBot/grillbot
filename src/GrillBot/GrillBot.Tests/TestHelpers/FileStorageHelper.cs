using GrillBot.App.Services.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using Moq;
using System.IO;
using System.Reflection;

namespace GrillBot.Tests.TestHelpers;

public static class FileStorageHelper
{
    public static FileStorageFactory Create(IConfiguration configuration = null)
    {
        configuration ??= ConfigurationHelper.CreateConfiguration();
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
