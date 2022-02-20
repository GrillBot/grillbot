using GrillBot.App.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardReactionHandler : ReactionEventHandler
{
    private GrillBotContextFactory DbFactory { get; }
    private DiscordSocketClient DiscordClient { get; }

    public PointsBoardReactionHandler(GrillBotContextFactory dbFactory, DiscordSocketClient discordClient)
    {
        DbFactory = dbFactory;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<PointsBoardMetadata>(message, emote, out var embed, out var metadata)) return false;

        var guild = DiscordClient.GetGuild(metadata.GuildId);
        if (guild == null) return false;

        var dbContext = DbFactory.Create();

        var query = dbContext.GuildUsers.AsNoTracking()
            .Where(o => o.GuildId == guild.Id.ToString() && o.Points > 0)
            .OrderByDescending(o => o.Points)
            .Select(o => new KeyValuePair<string, long>(o.UserId, o.Points));

        var pointsCount = await query.CountAsync();
        if (pointsCount == 0) return false;
        var pagesCount = (int)Math.Ceiling(pointsCount / 10.0);

        int newPage = GetPageNumber(metadata.Page, pagesCount, emote);
        if (newPage == metadata.Page) return false;

        var skip = (newPage == 0 ? 0 : newPage) * 10;
        var filteredQuery = query.Skip(skip).Take(10);
        var data = await filteredQuery.ToListAsync();

        await guild.DownloadUsersAsync();
        var resultEmbed = new PointsBoardBuilder()
            .WithBoard(user, guild, data, id => guild.GetUser(id), skip, newPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        await message.RemoveReactionAsync(emote, user);

        return true;
    }
}
