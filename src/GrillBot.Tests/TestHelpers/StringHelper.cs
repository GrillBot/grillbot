namespace GrillBot.Tests.TestHelpers;

public static class StringHelper
{
    public static void CheckTextParts(string text, params string[] parts)
    {
        Assert.IsFalse(string.IsNullOrEmpty(text));

        foreach (var part in parts)
            Assert.IsTrue(text.Contains(part), $"Text not contains part \"{part}\" ({text})");
    }
}
