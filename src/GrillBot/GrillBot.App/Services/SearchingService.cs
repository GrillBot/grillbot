using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.User;
using GrillBot.Common.Extensions;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using System.Security.Claims;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Database.Models;

namespace GrillBot.App.Services;

[Initializable]
public class SearchingService
{
    private UserService UserService { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SearchingService(IDiscordClient client, GrillBotDatabaseBuilder databaseBuilder, UserService userService,
        IMapper mapper)
    {
        UserService = userService;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task CreateAsync(IGuild guild, IGuildUser user, IGuildChannel channel, string message)
    {
        var content = CheckMessage(message);

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateRepositoryAsync(guild);
        await repository.User.GetOrCreateUserAsync(user);
        await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        await repository.Channel.GetOrCreateChannelAsync(channel);

        var entity = new SearchItem
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            UserId = user.Id.ToString(),
            MessageContent = content
        };

        await repository.AddAsync(entity);
        await repository.CommitAsync();
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
        var isAdmin = await UserService.CheckUserFlagsAsync(executor, UserFlags.BotAdmin) || executor.GuildPermissions.Administrator || executor.GuildPermissions.ManageMessages;

        await using var repository = DatabaseBuilder.CreateRepository();

        var search = await repository.Searching.FindSearchItemByIdAsync(id);
        if (search == null) return;

        if (!isAdmin && executor.Id != search.UserId.ToUlong())
            throw new UnauthorizedAccessException("Toto hledání jsi nezaložil ty a současně nemáš vyšší oprávnění hledání smazat.");

        repository.Remove(search);
        await repository.CommitAsync();
    }

    public async Task RemoveSearchesAsync(long[] ids)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var searches = await repository.Searching.FindSearchesByIdsAsync(ids);
        if (searches.Count == 0)
            return;

        repository.RemoveCollection(searches);
        await repository.CommitAsync();
    }

    public async Task<List<SearchingItem>> GetSearchListAsync(IGuild guild, ITextChannel channel, string messageQuery, int page)
    {
        var parameters = new GetSearchingListParams
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Pagination = new PaginatedParams
            {
                Page = page + 1,
                PageSize = EmbedBuilder.MaxFieldCount
            },
            Sort = new SortParams
            {
                OrderBy = "Id",
                Descending = false
            },
            MessageQuery = messageQuery
        };

        var data = await GetPaginatedListAsync(parameters, null);

        return data.Data.ConvertAll(o => new SearchingItem
        {
            DisplayName = o.User.Username,
            Id = o.Id,
            Message = o.Message
        });
    }

    private async Task<List<SearchingListItem>> GetListAsync(GetSearchingListParams parameters, ApiRequestContext apiRequestContext)
    {
        // TODO: Use ApiRequestContext.
        if (apiRequestContext?.IsPublic() == true)
        {
            var loggedUserId = apiRequestContext.GetUserId();
            var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);

            if (!string.IsNullOrEmpty(parameters.GuildId) && mutualGuilds.All(o => o.Id.ToString() != parameters.GuildId))
                parameters.GuildId = null;

            parameters.UserId = loggedUserId.ToString();
            parameters.MutualGuilds = mutualGuilds.ConvertAll(o => o.Id.ToString());
        }

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.Searching.FindSearchesAsync(parameters);

        var results = new List<SearchingListItem>();
        var synchronizedGuilds = new List<ulong>();

        foreach (var item in data)
        {
            var guild = await DiscordClient.GetGuildAsync(item.GuildId.ToUlong());
            if (guild == null)
            {
                repository.Remove(item);
                continue;
            }

            var channel = await guild.GetTextChannelAsync(item.ChannelId.ToUlong());
            if (channel == null)
            {
                repository.Remove(item);
                continue;
            }

            if (!synchronizedGuilds.Contains(guild.Id))
            {
                await guild.DownloadUsersAsync();
                synchronizedGuilds.Add(guild.Id);
            }

            var author = await guild.GetUserAsync(item.UserId.ToUlong());
            if (author == null)
            {
                repository.Remove(item);
                continue;
            }

            results.Add(Mapper.Map<SearchingListItem>(item));
        }

        await repository.CommitAsync();
        return results;
    }

    public async Task<PaginatedResponse<SearchingListItem>> GetPaginatedListAsync(GetSearchingListParams parameters, ApiRequestContext apiRequestContext)
    {
        var results = await GetListAsync(parameters, apiRequestContext);
        return PaginatedResponse<SearchingListItem>.Create(results, parameters.Pagination);
    }

    private async Task<int> GetItemsCountAsync(GetSearchingListParams parameters, ApiRequestContext apiRequestContext)
    {
        var results = await GetListAsync(parameters, apiRequestContext);
        return results.Count;
    }

    public async Task<int> GetItemsCountAsync(IGuild guild, ITextChannel channel, string messageQuery)
    {
        var parameters = new GetSearchingListParams
        {
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            Pagination = new PaginatedParams
            {
                Page = 1,
                PageSize = EmbedBuilder.MaxFieldCount
            },
            Sort = new SortParams
            {
                Descending = false,
                OrderBy = "Id"
            },
            MessageQuery = messageQuery
        };

        return await GetItemsCountAsync(parameters, null);
    }

    public async Task<Dictionary<long, string>> GenerateSuggestionsAsync(IGuildUser user, IGuild guild, IChannel channel)
    {
        var isBotAdmin = await UserService.CheckUserFlagsAsync(user, UserFlags.BotAdmin);
        var isAdmin = isBotAdmin || user.GuildPermissions.Administrator || user.GuildPermissions.ManageMessages;

        var parameters = new GetSearchingListParams
        {
            GuildId = guild.Id.ToString(),
            ChannelId = channel.Id.ToString(),
            UserId = isAdmin ? null : user.Id.ToString()
        };

        var items = await GetListAsync(parameters, null);

        return items.Take(25).ToDictionary(
            o => o.Id,
            o => $"#{o.Id} - " + (isAdmin ? $"{o.User.Username}#{o.User.Discriminator} - " : "") + $"({o.Message.Cut(20)})"
        );
    }
}
