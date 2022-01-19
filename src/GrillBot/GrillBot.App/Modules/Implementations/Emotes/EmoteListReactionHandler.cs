using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Modules.TextBased;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmoteListReactionHandler : ReactionEventHandler
{
    private GrillBotContextFactory DbFactory { get; }
    private DiscordSocketClient DiscordClient { get; }

    public EmoteListReactionHandler(GrillBotContextFactory dbFactory, DiscordSocketClient discordClient)
    {
        DbFactory = dbFactory;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<EmoteListMetadata>(message, emote, out var embed, out var metadata)) return false;

        var sortFunc = GetOrderFunction(metadata.SortBy, metadata.Desc);
        if (sortFunc == null) return false;

        using var context = DbFactory.Create();

        var query = EmotesModule.EmoteListSubModule.GetListQuery(context, metadata.OfUserId, sortFunc, null, null);
        var emotesCount = await query.CountAsync();
        if (emotesCount == 0) return false;

        int maxPages = (int)Math.Ceiling(emotesCount / (double)EmbedBuilder.MaxFieldCount);
        var newPage = GetPageNumber(metadata.Page, maxPages, emote);
        if (newPage == metadata.Page) return false;

        var skip = newPage * EmbedBuilder.MaxFieldCount;
        query = query.Skip(skip).Take(EmbedBuilder.MaxFieldCount);
        var data = await query.ToListAsync();

        var forUser = metadata.OfUserId == null ? null : await DiscordClient.FindUserAsync(metadata.OfUserId.Value);
        var resultEmbed = new EmbedBuilder()
            .WithEmoteList(data, user, forUser, metadata.IsPrivate, metadata.Desc, metadata.SortBy, newPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        if (!metadata.IsPrivate)
            await message.RemoveReactionAsync(emote, user);

        return true;
    }

    private static Func<IQueryable<IGrouping<string, EmoteStatisticItem>>, IQueryable<IGrouping<string, EmoteStatisticItem>>> GetOrderFunction(string sortBy, bool desc)
    {
        return sortBy switch
        {
            "count" => desc switch
            {
                true => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderByDescending(o => o.Sum(x => x.UseCount)).ThenByDescending(o => o.Max(x => x.LastOccurence)),
                false => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderBy(o => o.Sum(x => x.UseCount)).ThenBy(o => o.Max(x => x.LastOccurence))
            },
            "lastuse" => desc switch
            {
                true => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderByDescending(o => o.Max(x => x.LastOccurence)).ThenByDescending(o => o.Sum(x => x.UseCount)),
                false => (IQueryable<IGrouping<string, EmoteStatisticItem>> o) => o.OrderBy(o => o.Max(x => x.LastOccurence)).ThenBy(o => o.Sum(x => x.UseCount))
            },
            _ => null
        };
    }
}
