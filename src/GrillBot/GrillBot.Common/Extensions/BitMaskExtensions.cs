namespace GrillBot.Common.Extensions;

public static class BitMaskExtensions
{
    public static int UpdateFlags(this int currentValue, int bits, bool set)
    {
        if (set)
            currentValue |= bits;
        else
            currentValue &= ~bits;

        return currentValue;
    }
}
