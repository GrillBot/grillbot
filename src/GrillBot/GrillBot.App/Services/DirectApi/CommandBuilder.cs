using GrillBot.Data.Models.DirectApi;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.DirectApi;

public static class CommandBuilder
{
    public static DirectMessageCommand CreateHelpCommand(ulong userId)
    {
        return new DirectMessageCommand("Help")
            .WithParameter("user_id", userId);
    }

    public static DirectMessageCommand CreateKarmaCommand(SortParams sort, PaginatedParams pagination)
    {
        return new DirectMessageCommand("Karma")
            .WithParameter("order", sort.Descending ? "desc" : "asc")
            .WithParameter("board", sort.OrderBy!.ToLower())
            .WithParameter("page", pagination.Page);
    }
}
