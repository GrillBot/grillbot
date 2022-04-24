using System;

namespace GrillBot.Data.Extensions;

public static class StringExtensions
{
    public static int ToInt(this string str) => Convert.ToInt32(str);
    public static ulong ToUlong(this string str) => Convert.ToUInt64(str);
}
