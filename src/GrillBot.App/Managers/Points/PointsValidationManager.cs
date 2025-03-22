using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;

namespace GrillBot.App.Managers.Points;

public class PointsValidationManager
{
    private readonly IDiscordClient _discordClient;
    private readonly GrillBotDatabaseBuilder _databaseBuilder;

    public PointsValidationManager(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        _discordClient = discordClient;
        _databaseBuilder = databaseBuilder;
    }

    public bool CanIncrementPoints(IMessage message, IUser? reactionUser = null)
    {
        var user = reactionUser ?? message.Author;
        if (!user.IsUser())
            return false; // IsBot

        if (message.Channel is not ITextChannel)
            return false;

        if (message.IsCommand(_discordClient.CurrentUser)) // CommandCheck
            return false;

        if (message.Author.Id == reactionUser?.Id) // SelfReaction
            return false;

        return true;
    }

    public async Task<bool> IsUserAcceptableAsync(IUser user)
    {
        using var repository = _databaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        return userEntity?.HaveFlags(UserFlags.NotUser) == false;
    }

    public static bool IsMissingData(Dictionary<string, string[]> validationErrors)
    {
        var errors = validationErrors.SelectMany(o => o.Value).Distinct().ToList();
        return errors.Contains("UnknownChannel") || errors.Contains("UnknownUser");
    }
}
