using System;
using GrillBot.App.Services.Suggestion;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Suggestion;

[TestClass]
public class EmoteSuggestionEmbedBuilderTests
{
    [TestMethod]
    public void Build_ApprovedVote()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var entity = new EmoteSuggestion
        {
            CreatedAt = DateTime.Now,
            EmoteName = "Emote",
            ApprovedForVote = true
        };

        var builder = new EmoteSuggestionEmbedBuilder(entity, user);
        var result = builder.Build();

        Assert.IsNotNull(result.Author);
        Assert.IsNotNull(result.Timestamp);
        Assert.AreEqual(1, result.Fields.Length);
        Assert.IsNotNull(result.Description);
    }

    [TestMethod]
    public void Build_WithDescription()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var entity = new EmoteSuggestion
        {
            CreatedAt = DateTime.Now,
            EmoteName = "Emote",
            Description = "Popis"
        };

        var builder = new EmoteSuggestionEmbedBuilder(entity, user);
        var result = builder.Build();

        Assert.IsNotNull(result.Author);
        Assert.IsNotNull(result.Timestamp);
        Assert.AreEqual(2, result.Fields.Length);
    }
}
