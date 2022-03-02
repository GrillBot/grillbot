using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Unverify;

namespace GrillBot.App.Modules.Implementations.Unverify;

public class UnverifyListReactionHandler : ReactionEventHandler
{
    private DiscordSocketClient DiscordClient { get; }
    private UnverifyService UnverifyService { get; }

    public UnverifyListReactionHandler(DiscordSocketClient discordClient, UnverifyService unverifyService)
    {
        UnverifyService = unverifyService;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<UnverifyListMetadata>(message, emote, out IEmbed embed, out var metadata)) return false;

        var guild = DiscordClient.GetGuild(metadata.GuildId);
        if (guild == null) return false;

        var maxPages = await UnverifyService.GetUnverifyCountsOfGuildAsync(guild);
        var nextPage = GetNextPageNumber(metadata.Page, maxPages, emote);
        if (nextPage == metadata.Page) return false;

        var unverify = await UnverifyService.GetCurrentUnverifyAsync(guild, nextPage);
        if (unverify == null) return false;

        var resultEmbed = new EmbedBuilder()
            .WithUnverifyList(unverify, guild, user, nextPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        await message.RemoveReactionAsync(emote, user);
        return true;
    }
}
