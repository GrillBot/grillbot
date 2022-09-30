using System.IO;
using GrillBot.Common.FileStorage;
using Moq;

namespace GrillBot.Tests.Infrastructure;

public class FileStorageMock : FileStorageFactory
{
    private List<(string category, string filename, FileInfo info)> Files { get; } = new()
    {
        ("DeletedAttachments", "Temp.txt", new FileInfo("Temp.txt")),
        ("DeletedAttachments", "Temp.unknown", new FileInfo("Temp.unknown")),
        ("DeletedAttachments", "Temporary.txt", new FileInfo("Temporary.txt")),
        ("Clearing", null, new FileInfo("File.xml")),
        ("Common", "LastErrorDate.txt", new FileInfo("LastErrorDateTest.txt")),
        ("Peepolove", $"{Consts.UserId}_{Consts.Discriminator}_256.png", new FileInfo($"{Consts.UserId}_{Consts.Discriminator}_256.png")),
        ("Peepoangry", $"{Consts.UserId}_{Consts.Discriminator}_64.png", new FileInfo($"{Consts.UserId}_{Consts.Discriminator}_64.png")),
    };

    public FileStorageMock(IConfiguration configuration) : base(configuration)
    {
    }

    public override IFileStorage Create(string categoryName)
    {
        var mock = new Mock<IFileStorage>();

        foreach (var file in Files)
        {
            if (string.IsNullOrEmpty(file.filename))
                mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == file.category), It.IsAny<string>())).ReturnsAsync(file.info);
            else
                mock.Setup(o => o.GetFileInfoAsync(It.Is<string>(c => c == file.category), It.Is<string>(f => f == file.filename))).ReturnsAsync(file.info);
        }

        return mock.Object;
    }
}
