using System;
using SysIO = System.IO;

namespace GrillBot.Data.Infrastructure.IO
{
    public sealed class TemporaryFile : IDisposable
    {
        public string Path { get; }

        public TemporaryFile(string extension)
        {
            Path = SysIO.Path.Combine(SysIO.Path.GetTempPath(), $"{SysIO.Path.GetRandomFileName()}.{extension}");
        }

        public void Dispose()
        {
            if (SysIO.File.Exists(Path))
                SysIO.File.Delete(Path);
        }

        public override string ToString() => Path;
    }
}
