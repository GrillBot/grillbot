using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Emotes;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmoteListReactionHandler : ReactionEventHandler
{
    private DiscordSocketClient DiscordClient { get; }
    private EmoteService EmoteService { get; }

    public EmoteListReactionHandler(DiscordSocketClient discordClient, EmoteService emoteService)
    {
        DiscordClient = discordClient;
        EmoteService = emoteService;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<EmoteListMetadata>(message, emote, out var embed, out var metadata)) return false;

        var guild = DiscordClient.GetGuild(metadata.GuildId);
        var ofUser = metadata.OfUserId == null ? null : await DiscordClient.FindUserAsync(metadata.OfUserId.Value);
        var emotesCount = await EmoteService.GetEmotesCountAsync(guild, ofUser);
        if (emotesCount == 0) return false;

        int maxPages = (int)Math.Ceiling(emotesCount / (double)EmbedBuilder.MaxFieldCount);
        var newPage = GetNextPageNumber(metadata.Page, maxPages, emote);
        if (newPage == metadata.Page) return false;

        var skip = newPage * EmbedBuilder.MaxFieldCount;
        var data = await EmoteService.GetEmoteListAsync(guild, ofUser, metadata.SortQuery, skip);

        var resultEmbed = new EmbedBuilder()
            .WithEmoteList(data, user, ofUser, guild, metadata.SortQuery, newPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        await message.RemoveReactionAsync(emote, user);

        return true;
    }
}
