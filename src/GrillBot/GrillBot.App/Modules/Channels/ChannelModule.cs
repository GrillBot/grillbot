using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Modules.Channels;
using GrillBot.Data;
using GrillBot.Data.Models;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    [Group("channel")]
    [Name("Správa kanálů")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze provést pouze na serveru.")]
    public class ChannelModule : Infrastructure.ModuleBase
    {
        private GrillBotContextFactory DbFactory { get; }
        private IMemoryCache MemoryCache { get; }
        private IConfiguration Configuration { get; }

        public ChannelModule(GrillBotContextFactory dbFactory, IMemoryCache memoryCache, IConfiguration configuration)
        {
            DbFactory = dbFactory;
            MemoryCache = memoryCache;
            Configuration = configuration;
        }

        [Command("board")]
        [Summary("Získání TOP 10 kompletních statistik z kanálů, kam má uživatel přístup.")]
        public async Task GetChannelBoardAsync()
        {
            await Context.Guild.DownloadUsersAsync();
            if (Context.User is not SocketGuildUser user)
                user = Context.Guild.GetUser(Context.User.Id);

            var availableChannels = Context.Guild.GetAvailableTextChannelsFor(user).Select(o => o.Id.ToString()).ToList();

            using var dbContext = DbFactory.Create();

            var query = dbContext.UserChannels.AsQueryable()
                .Where(o => o.GuildId == Context.Guild.Id.ToString() && availableChannels.Contains(o.Id) && o.Count > 0);

            var groupedDataQuery = query.GroupBy(o => new { o.GuildId, o.Id }).Select(o => new
            {
                ChannelId = o.Key.Id,
                Count = o.Sum(x => x.Count)
            }).OrderByDescending(o => o.Count).Select(o => new KeyValuePair<string, long>(o.ChannelId, o.Count));

            if (!await groupedDataQuery.AnyAsync())
            {
                await ReplyAsync("Ještě nebyly zachyceny žádné události ukazující aktivitu serveru.");
                return;
            }

            var groupedData = await groupedDataQuery.Take(10).ToListAsync();

            var embed = new ChannelboardBuilder()
                .WithChannelboard(Context.User, Context.Guild, groupedData, id => Context.Guild.GetTextChannel(id), 0);

            var message = await ReplyAsync(embed: embed.Build());
            await message.AddReactionsAsync(Emojis.PaginationEmojis);
        }

        [Command]
        [Summary("Získání statistiky zpráv z jednotlivého kanálu.")]
        public async Task GetStatisticsOfChannelAsync(SocketGuildChannel channel)
        {
            await Context.Guild.DownloadUsersAsync();
            if (!channel.HaveAccess(Context.User is SocketGuildUser sgu ? sgu : Context.Guild.GetUser(Context.User.Id)))
            {
                await ReplyAsync("Promiň, ale do tohoto kanálu nemáš přístup.");
                return;
            }

            using var dbContext = DbFactory.Create();

            var channelDataQuery = dbContext.UserChannels.AsQueryable()
                .Where(o => o.GuildId == Context.Guild.Id.ToString() && o.Id == channel.Id.ToString() && o.Count > 0);

            var groupedDataQuery = channelDataQuery.GroupBy(o => new { o.GuildId, o.Id }).Select(o => new
            {
                ChannelId = o.Key.Id,
                Count = o.Sum(x => x.Count),
                FirstMessageAt = o.Min(x => x.FirstMessageAt),
                LastMessageAt = o.Max(x => x.LastMessageAt)
            });

            var channelData = groupedDataQuery.FirstOrDefault();
            if (channelData == null)
            {
                await ReplyAsync("Promiň, ale zatím nemám informace o aktivitě v tomto kanálu.");
                return;
            }

            var topTenQuery = channelDataQuery.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastMessageAt).Take(10);
            var topTenData = await topTenQuery.ToListAsync();
            var topTenFormatted = string.Join("\n", topTenData.Select((o, i) =>
            {
                var user = Context.Guild.GetUser(Convert.ToUInt64(o.UserId));
                return $"**{i + 1,2}.** {user.GetDisplayName()} ({FormatHelper.FormatMessagesToCzech(o.Count)})";
            }));

            var embed = new EmbedBuilder()
                .WithFooter(Context.User)
                .WithAuthor("Statistika aktivity v kanálu")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithTitle($"#{channel.Name}")
                .AddField("Vytvořeno", channel.CreatedAt.LocalDateTime.ToCzechFormat(), true)
                .AddField("Počet zpráv", FormatHelper.FormatMessagesToCzech(channelData.Count), true)
                .AddField("První zpráva", channelData.LastMessageAt.ToCzechFormat(), true)
                .AddField("Poslední zpráva", channelData.FirstMessageAt.ToCzechFormat(), true)
                .AddField("Počet uživatelů", FormatHelper.FormatMembersToCzech(channel.Users.Count), true)
                .AddField("Počet oprávnění", FormatHelper.FormatPermissionstoCzech(channel.PermissionOverwrites.Count), true)
                .AddField("TOP 10 uživatelů", topTenFormatted, false);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("web")]
        [Summary("Získání kompletní statistiky zpráv pro uživatele.")]
        public async Task GetChannelBoardWebAsync()
        {
            var sessionId = Guid.NewGuid().ToString().Replace("-", "");

            var metadata = new ChannelboardWebMetadata()
            {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id
            };

            MemoryCache.Set(sessionId, metadata);
            var url = string.Format(Configuration["Channelboard:Web:Address"], sessionId);
            var message = $"Zda máš odkaz pro přístup na kompletní statistiku kanálů pro server **{Context.Guild.Name}**: <{url}>";

            try
            {
                await Context.User.SendMessageAsync(message);
            }
            catch (HttpException ex) when (ex.DiscordCode == 50007)
            {
                // User have disabled DMs.
            }
        }
    }
}
