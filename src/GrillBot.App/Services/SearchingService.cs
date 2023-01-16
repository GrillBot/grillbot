using GrillBot.App.Infrastructure;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

[Initializable]
public class SearchingService
{
    private UserManager UserManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IServiceProvider ServiceProvider { get; }

    public SearchingService(GrillBotDatabaseBuilder databaseBuilder, UserManager userManager, IServiceProvider serviceProvider)
    {
        UserManager = userManager;
        DatabaseBuilder = databaseBuilder;
        ServiceProvider = serviceProvider;
    }

    public async Task CreateAsync(IGuild guild, IGuildUser user, IGuildChannel channel, string message)
    {
        var content = CheckMessage(message);

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guild);
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

        const int limit = EmbedFieldBuilder.MaxFieldValueLength - 3;
        if (message.Length > limit)
            throw new ValidationException($"Zpráva nesmí být delší, než {limit} znaků.");

        return message;
    }

    public async Task RemoveSearchAsync(long id, IGuildUser executor)
    {
        var isAdmin = await UserManager.CheckFlagsAsync(executor, UserFlags.BotAdmin) || executor.GuildPermissions.Administrator || executor.GuildPermissions.ManageMessages;

        await using var repository = DatabaseBuilder.CreateRepository();

        var search = await repository.Searching.FindSearchItemByIdAsync(id);
        if (search == null) return;

        if (!isAdmin && executor.Id != search.UserId.ToUlong())
            throw new UnauthorizedAccessException("Toto hledání jsi nezaložil ty a současně nemáš vyšší oprávnění hledání smazat.");

        repository.Remove(search);
        await repository.CommitAsync();
    }

    public async Task<Dictionary<long, string>> GenerateSuggestionsAsync(IGuildUser user, IGuild guild, IChannel channel, string locale)
    {
        var isBotAdmin = await UserManager.CheckFlagsAsync(user, UserFlags.BotAdmin);
        var isAdmin = isBotAdmin || user.GuildPermissions.Administrator || user.GuildPermissions.ManageMessages;

        var parameters = new GetSearchingListParams
        {
            GuildId = guild.Id.ToString(),
            ChannelId = channel.Id.ToString(),
            UserId = isAdmin ? null : user.Id.ToString(),
            Pagination = { Page = 0, PageSize = 25 },
            Sort = { Descending = false, OrderBy = "Id" }
        };

        using var scope = ServiceProvider.CreateScope();
        var action = scope.ServiceProvider.GetRequiredService<Actions.Api.V1.Searching.GetSearchingList>();
        action.UpdateContext(locale, user);
        var items = await action.ProcessAsync(parameters);

        return items.Data.ToDictionary(
            o => o.Id,
            o => $"#{o.Id} - " + (isAdmin ? $"{o.User.Username}#{o.User.Discriminator} - " : "") + $"({o.Message.Cut(20)})"
        );
    }
}
