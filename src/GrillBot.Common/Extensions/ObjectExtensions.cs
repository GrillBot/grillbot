namespace GrillBot.Common.Extensions;

public static class ObjectExtensions
{
    public static TResult? GetPropertyValue<TObject, TResult>(this TObject? obj, Func<TObject, TResult> selector)
        => obj is null ? default : selector(obj);
}
