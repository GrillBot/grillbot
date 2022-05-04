using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.DirectApi;

namespace GrillBot.App.Services.DirectApi;

public static class CommandBuilder
{
    public static DirectMessageCommand CreateHelpCommand(ulong userId)
    {
        return new DirectMessageCommand("Help")
            .WithParameter("user_id", userId);
    }

    public static DirectMessageCommand CreateKarmaCommand(string board, SortParams sort, PaginatedParams pagination)
    {
        return new DirectMessageCommand("Karma")
            .WithParameter("order", sort.Descending ? "desc" : "asc")
            .WithParameter("board", board)
            .WithParameter("page", pagination.Page);
    }
}
