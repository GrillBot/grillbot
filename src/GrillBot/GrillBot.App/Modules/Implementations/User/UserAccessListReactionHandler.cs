using GrillBot.App.Infrastructure;
using GrillBot.App.Modules.TextBased.User;

namespace GrillBot.App.Modules.Implementations.User;

public class UserAccessListReactionHandler : ReactionEventHandler
{
    private DiscordSocketClient DiscordClient { get; }

    public UserAccessListReactionHandler(DiscordSocketClient discordClient)
    {
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<UserAccessListMetadata>(message, emote, out var embed, out var metadata)) return false;

        var guild = DiscordClient.GetGuild(metadata.GuildId);
        if (guild == null) return false;

        await guild.DownloadUsersAsync();
        var forUser = guild.GetUser(metadata.ForUserId);
        if (forUser == null) return false;

        var newPage = GetNextPageNumber(metadata.Page, int.MaxValue, emote);
        if (newPage == metadata.Page) return false;

        var channels = UserModule.GetUserVisibleChannels(guild, forUser)
            .Skip(newPage * UserModule.UserAccessMaxCountPerPage)
            .Take(UserModule.UserAccessMaxCountPerPage)
            .ToList();
        if (channels.Count == 0) return false;

        var resultEmbed = new EmbedBuilder()
            .WithUserAccessList(channels, forUser, user, guild, newPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        await message.RemoveReactionAsync(emote, user);

        return true;
    }
}
