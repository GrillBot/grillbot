using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Commands.Searching;

public class RemoveSearch : CommandAction
{
    private UserManager UserManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ITextsManager Texts { get; }

    public string? ErrorMessage { get; private set; }

    public RemoveSearch(UserManager userManager, GrillBotDatabaseBuilder databaseBuilder, ITextsManager texts)
    {
        UserManager = userManager;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
    }

    public async Task ProcessAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var search = await repository.Searching.FindSearchItemByIdAsync(id);
        if (search == null || !await CheckPermissionsAsync(search)) return;

        repository.Remove(search);
        await repository.CommitAsync();
    }

    private async Task<bool> CheckPermissionsAsync(SearchItem search)
    {
        var executingUser = await GetExecutingUserAsync();
        var haveGuildPerms = executingUser.GuildPermissions.Administrator || executingUser.GuildPermissions.ManageMessages;
        var isAdmin = haveGuildPerms || await UserManager.CheckFlagsAsync(Context.User, UserFlags.BotAdmin);
        var canRemove = isAdmin || executingUser.Id == search.UserId.ToUlong();

        if (!canRemove)
            ErrorMessage = Texts["SearchingModule/RemoveSearch/InsufficientPermissions", Locale];
        return canRemove;
    }
}
