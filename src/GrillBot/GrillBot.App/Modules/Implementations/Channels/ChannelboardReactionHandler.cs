using Discord;
using Discord.WebSocket;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Infrastructure;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Modules.Implementations.Channels;

public class ChannelboardReactionHandler : ReactionEventHandler
{
    private GrillBotContextFactory DbFactory { get; }
    private DiscordSocketClient DiscordClient { get; }

    public ChannelboardReactionHandler(GrillBotContextFactory dbFactory, DiscordSocketClient discordClient)
    {
        DbFactory = dbFactory;
        DiscordClient = discordClient;
    }

    public override async Task<bool> OnReactionAddedAsync(IUserMessage message, IEmote emote, IUser user)
    {
        if (!TryGetEmbedAndMetadata<ChannelboardMetadata>(message, emote, out var embed, out var metadata)) return false;

        var guild = DiscordClient.GetGuild(metadata.GuildId);
        if (guild == null) return false;

        await guild.DownloadUsersAsync();
        var guildUser = user is SocketGuildUser sgu ? sgu : guild.GetUser(user.Id);
        var availableChannels = guild.GetAvailableTextChannelsFor(guildUser).Select(o => o.Id.ToString()).ToList();

        using var dbContext = DbFactory.Create();

        var query = dbContext.UserChannels.AsQueryable()
            .Where(o => o.GuildId == guild.Id.ToString() && availableChannels.Contains(o.Id) && o.Count > 0);

        var groupedDataQuery = query.GroupBy(o => new { o.GuildId, o.Id }).Select(o => new
        {
            ChannelId = o.Key.Id,
            Count = o.Sum(x => x.Count)
        }).OrderByDescending(o => o.Count).Select(o => new KeyValuePair<string, long>(o.ChannelId, o.Count));

        var channelsCount = await groupedDataQuery.CountAsync();
        if (channelsCount == 0) return false;

        int newPage = GetPageNumber(metadata.Page, channelsCount, emote);
        if (newPage == metadata.Page) return false;

        var skip = (newPage == 0 ? 0 : newPage) * 10;
        var groupedData = await groupedDataQuery.Skip(skip).Take(10).ToListAsync();

        var resultEmbed = new ChannelboardBuilder()
            .WithChannelboard(guildUser, guild, groupedData, id => guild.GetTextChannel(id), skip, newPage);

        await message.ModifyAsync(o => o.Embed = resultEmbed.Build());
        await message.RemoveReactionAsync(emote, user);

        return true;
    }
}
