using Discord;
using GrillBot.App.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;

namespace GrillBot.App.Services;

public class MockingService
{
    private Emote MockingEmote { get; }
    private Random Random { get; }

    // MaxMessageSize - 2xMocking emotes - Spaces
    private int MaxMessageLength => DiscordConfig.MaxMessageSize - (2 * MockingEmote.ToString().Length) - 2;

    // Get maximum range value for a random number generator that decides if the char should be uppercase.
    // When the char is uppercased, the index is set to last element.
    // The index is decremented for each lowercased char
    //
    // This means the char following uppercased char has 20% (1/5) chance of changing to uppercase.
    // If it's not changed, then the next char has 50% (1/2) chance of being uppercased. Finally if
    // even the second char is not uppercased, the next valid char has 100% chance.
    private readonly int[] MockRandomCoefficient = { 1, 2, 5 };

    public MockingService(IConfiguration configuration, RandomizationService randomizationService)
    {
        MockingEmote = Emote.Parse(configuration.GetValue<string>("Discord:Emotes:Mocking"));
        Random = randomizationService.GetOrCreateGenerator("Mocking");
    }

    public string CreateMockingString(string original)
    {
        original = original.ToLower().Cut(MaxMessageLength, true);

        var resultBuilder = new StringBuilder();
        var coeffIndex = 0;

        foreach (var c in original)
        {
            // Letter 'i' cannot be uppercased and letter 'l' should be always uppercased.
            // This feature is here to prevent confusion of lowercase 'l' and uppercase 'i'
            if (char.IsLetter(c) && c != 'i' && (c == 'l' || Random.Next(MockRandomCoefficient[coeffIndex]) == 0))
            {
                resultBuilder.Append(char.ToUpperInvariant(c));
                coeffIndex = MockRandomCoefficient.Length - 1;
                continue;
            }

            resultBuilder.Append(c);

            if (coeffIndex > 0) coeffIndex--;
        }

        return $"{MockingEmote} {resultBuilder} {MockingEmote}";
    }
}
