using GrillBot.App.Managers;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Searching;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Commands.Searching;

public class GetSuggestions : CommandAction
{
    private UserManager UserManager { get; }
    private Api.V1.Searching.GetSearchingList ApiAction { get; }

    public GetSuggestions(UserManager userManager, Api.V1.Searching.GetSearchingList apiAction)
    {
        UserManager = userManager;
        ApiAction = apiAction;
    }

    public async Task<List<AutocompleteResult>> ProcessAsync()
    {
        ApiAction.UpdateContext(Locale, Context.User);

        var isAdmin = await IsAdminAsync();
        var parameters = new GetSearchingListParams
        {
            Pagination = { Page = 0, PageSize = 25 },
            Sort = { Descending = false, OrderBy = "Id" },
            ChannelId = Context.Channel.Id.ToString(),
            GuildId = Context.Guild.Id.ToString(),
            UserId = isAdmin ? null : Context.User.Id.ToString()
        };

        var items = await ApiAction.ProcessAsync(parameters);
        return items.Data.Select(o => new AutocompleteResult(
            $"#{o.Id} - " + (isAdmin ? $"{o.User.Username}#{o.User.Discriminator} - " : "") + $"({o.Message.Cut(20)})",
            o.Id
        )).ToList();
    }

    private async Task<bool> IsAdminAsync()
    {
        var executingUser = await GetExecutingUserAsync();
        return executingUser.GuildPermissions.Administrator || executingUser.GuildPermissions.ManageMessages ||
               await UserManager.CheckFlagsAsync(executingUser, UserFlags.BotAdmin);
    }
}
