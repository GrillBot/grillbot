using System;
using System.IO;
using System.Text;

namespace GrillBot.Data.Extensions
{
    static public class ExceptionExtensions
    {
        static public MemoryStream ToMemoryStream(this Exception exception)
        {
            var bytes = Encoding.UTF8.GetBytes(exception.ToString());
            return new MemoryStream(bytes);
        }
    }
}
