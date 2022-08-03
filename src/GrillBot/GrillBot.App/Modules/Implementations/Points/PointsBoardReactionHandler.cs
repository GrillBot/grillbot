using GrillBot.App.Infrastructure;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardReactionHandler : ReactionEventHandler
{
    private GrillBotDatabaseBuilder DbFactory { get; }
    private DiscordSocketClient DiscordClient { get; }

    public PointsBoardReactionHandler(GrillBotDatabaseBuilder dbFactory, DiscordSocketClient discordClient)
    {
        DbFactory = dbFactory;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<PointsBoardMetadata>(message, emote, out _, out var metadata)) return false;

        var guild = DiscordClient.GetGuild(metadata.GuildId);
        if (guild == null) return false;

        await using var repository = DbFactory.CreateRepository();
        var pointsBoardData = await repository.Points.GetPointsBoardDataAsync(new[] { guild.Id.ToString() });

        var pointsCount = pointsBoardData.Count;
        if (pointsCount == 0) return false;
        var pagesCount = (int)Math.Ceiling(pointsCount / 10.0);

        var newPage = GetNextPageNumber(metadata.Page, pagesCount, emote);
        if (newPage == metadata.Page) return false;

        var skip = (newPage == 0 ? 0 : newPage) * 10;
        var data = pointsBoardData.Skip(skip).Take(10).ToList();

        await guild.DownloadUsersAsync();
        var resultEmbed = new PointsBoardBuilder().WithBoard(user, guild, data, skip, newPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        await message.RemoveReactionAsync(emote, user);

        return true;
    }
}
