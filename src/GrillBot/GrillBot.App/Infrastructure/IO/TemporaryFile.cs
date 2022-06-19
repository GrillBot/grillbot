namespace GrillBot.App.Infrastructure.IO;

public sealed class TemporaryFile : IDisposable
{
    public string Path { get; }

    public TemporaryFile(string extension)
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{System.IO.Path.GetRandomFileName()}.{extension}");
    }

    public void Dispose()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }

    public override string ToString() => Path;
}
