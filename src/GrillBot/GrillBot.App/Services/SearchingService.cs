using AutoMapper;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.User;
using GrillBot.Data.Extensions;
using GrillBot.Data.Helpers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using System.Security.Claims;

namespace GrillBot.App.Services;

[Initializable]
public class SearchingService : ServiceBase
{
    private UserService UserService { get; }

    public SearchingService(DiscordSocketClient client, GrillBotContextFactory dbFactory, UserService userService,
        IMapper mapper) : base(client, dbFactory, null, null, mapper)
    {
        UserService = userService;
    }

    public async Task CreateAsync(IGuild guild, IUser user, IChannel channel, string message)
    {
        var content = CheckMessage(message);

        using var context = DbFactory.Create();

        await context.InitUserAsync(user);
        await context.InitGuildAsync(guild);
        await context.InitGuildChannelAsync(guild, channel, DiscordHelper.GetChannelType(channel) ?? ChannelType.DM);
        await context.InitGuildUserAsync(guild, user as IGuildUser);

        var entity = new SearchItem()
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            UserId = user.Id.ToString(),
            MessageContent = content
        };

        await context.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    private static string CheckMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new ValidationException("Obsah zprávy nesmí být prázdný.");

        message = message.Trim();
        if (message.Length > EmbedFieldBuilder.MaxFieldValueLength)
            throw new ValidationException($"Zpráva nesmí být delší, než {EmbedFieldBuilder.MaxFieldValueLength} znaků.");

        return message;
    }

    public async Task RemoveSearchAsync(long id, IGuildUser executor)
    {
        var isAdmin = (await UserService.IsUserBotAdminAsync(executor)) || executor.GuildPermissions.Administrator || executor.GuildPermissions.ManageMessages;

        using var context = DbFactory.Create();

        var search = await context.SearchItems.AsQueryable().FirstOrDefaultAsync(o => o.Id == id);
        if (search == null) return;

        if (!isAdmin && executor.Id != search.UserId.ToUlong())
            throw new UnauthorizedAccessException("Toto hledání jsi nezaložil ty a současně nemáš vyšší oprávnění hledání smazat.");

        context.Remove(search);
        await context.SaveChangesAsync();
    }

    public async Task RemoveSearchesAsync(long[] ids)
    {
        using var context = DbFactory.Create();

        var searches = await context.SearchItems.AsQueryable()
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();

        context.RemoveRange(searches);
        await context.SaveChangesAsync();
    }

    public async Task<List<SearchingItem>> GetSearchListAsync(IGuild guild, ITextChannel channel, string messageQuery, int page)
    {
        var parameters = new GetSearchingListParams()
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Pagination = new()
            {
                Page = page + 1,
                PageSize = EmbedBuilder.MaxFieldCount
            },
            Sort = new()
            {
                OrderBy = "Id",
                Descending = false
            },
            MessageQuery = messageQuery
        };

        var data = await GetPaginatedListAsync(parameters, null, CancellationToken.None);

        return data.Data.ConvertAll(o => new SearchingItem()
        {
            DisplayName = o.User.Username,
            Id = o.Id,
            Message = o.Message
        });
    }

    public async Task<List<SearchingListItem>> GetListAsync(GetSearchingListParams parameters, ClaimsPrincipal loggedUser,
        CancellationToken cancellationToken)
    {
        if (loggedUser?.HaveUserPermission() == true)
        {
            var loggedUserId = loggedUser.GetUserId();
            var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);

            if (!string.IsNullOrEmpty(parameters.GuildId) && !mutualGuilds.Any(o => o.Id.ToString() == parameters.GuildId))
                parameters.GuildId = null;

            parameters.UserId = loggedUserId.ToString();
            parameters.MutualGuilds = mutualGuilds.ConvertAll(o => o.Id.ToString());
        }

        using var context = DbFactory.Create();

        var query = context.CreateQuery(parameters);
        var data = await query.ToListAsync(cancellationToken);

        var results = new List<SearchingListItem>();
        var synchronizedGuilds = new List<ulong>();

        foreach (var item in data)
        {
            var guild = DiscordClient.GetGuild(item.GuildId.ToUlong());
            if (guild == null)
            {
                CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                continue;
            }

            var channel = guild.GetTextChannel(item.ChannelId.ToUlong());
            if (channel == null)
            {
                CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                continue;
            }

            if (!synchronizedGuilds.Contains(guild.Id))
            {
                await guild.DownloadUsersAsync();
                synchronizedGuilds.Add(guild.Id);
            }

            var author = guild.GetUser(item.UserId.ToUlong());
            if (author == null)
            {
                CommonHelper.SuppressException<InvalidOperationException>(() => context.Remove(item));
                continue;
            }

            results.Add(Mapper.Map<SearchingListItem>(item));
        }

        await context.SaveChangesAsync(cancellationToken);
        return results;
    }

    public async Task<PaginatedResponse<SearchingListItem>> GetPaginatedListAsync(GetSearchingListParams parameters, ClaimsPrincipal loggedUser,
        CancellationToken cancellationToken)
    {
        var results = await GetListAsync(parameters, loggedUser, cancellationToken);
        return PaginatedResponse<SearchingListItem>.Create(results, parameters.Pagination);
    }

    public async Task<int> GetItemsCountAsync(GetSearchingListParams parameters, ClaimsPrincipal loggedUser,
        CancellationToken cancellationToken)
    {
        var results = await GetListAsync(parameters, loggedUser, cancellationToken);
        return results.Count;
    }

    public async Task<int> GetItemsCountAsync(IGuild guild, ITextChannel channel, string messageQuery)
    {
        var parameters = new GetSearchingListParams()
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Pagination = new()
            {
                Page = 1,
                PageSize = EmbedBuilder.MaxFieldCount
            },
            Sort = new()
            {
                Descending = false,
                OrderBy = "Id"
            },
            MessageQuery = messageQuery
        };

        return await GetItemsCountAsync(parameters, null, CancellationToken.None);
    }

    public async Task<Dictionary<long, string>> GenerateSuggestionsAsync(IGuildUser user, IGuild guild, IChannel channel)
    {
        var isBotAdmin = await UserService.IsUserBotAdminAsync(user);
        var isAdmin = isBotAdmin || user.GuildPermissions.Administrator || user.GuildPermissions.ManageMessages;

        var parameters = new GetSearchingListParams()
        {
            GuildId = guild.Id.ToString(),
            ChannelId = channel.Id.ToString(),
            UserId = isAdmin ? null : user.Id.ToString()
        };

        var items = await GetListAsync(parameters, null, CancellationToken.None);

        return items.Take(25).ToDictionary(
            o => o.Id,
            o => $"#{o.Id} - " + (isAdmin ? $"{o.User.Username}#{o.User.Discriminator} - " : "") + $"({o.Message.Cut(20)})"
        );
    }
}
