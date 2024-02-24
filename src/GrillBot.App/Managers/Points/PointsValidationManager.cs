using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<bool> CanIncrementPointsAsync(IMessage message, IUser? reactionUser = null)
    {
        await using var repository = _databaseBuilder.CreateRepository();

        var user = reactionUser ?? message.Author;
        if (!await IsValidForIncrementAsync(repository, user)) // IsBot, DisabledPoints
            return false;

        if (!await IsValidForIncrementAsync(repository, message.Channel)) // IsDeleted, DisabledPoints
            return false;

        if (message.IsCommand(_discordClient.CurrentUser)) // CommandCheck
            return false;

        if (message.Author.Id == reactionUser?.Id) // SelfReaction
            return false;

        return true;
    }

    private static async Task<bool> IsValidForIncrementAsync(GrillBotRepository repository, IUser user)
    {
        if (!user.IsUser())
            return false;

        var entity = await repository.User.FindUserAsync(user, true);
        return entity?.HaveFlags(UserFlags.PointsDisabled) == false;
    }

    private static async Task<bool> IsValidForIncrementAsync(GrillBotRepository repository, IChannel channel)
    {
        if (channel is not ITextChannel textChannel)
            return false;

        var entity = await repository.Channel.FindChannelByIdAsync(textChannel.Id, textChannel.GuildId, true);
        return entity?.HasFlag(ChannelFlag.PointsDeactivated) == false;
    }

    public async Task<bool> IsUserAcceptableAsync(IUser user)
    {
        await using var repository = _databaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        return userEntity?.HaveFlags(UserFlags.NotUser) == false && !userEntity.HaveFlags(UserFlags.PointsDisabled);
    }

    public static bool IsMissingData(ValidationProblemDetails? details)
    {
        if (details is null) return false;

        var errors = details.Errors.SelectMany(o => o.Value).Distinct().ToList();
        return errors.Contains("UnknownChannel") || errors.Contains("UnknownUser");
    }
}
