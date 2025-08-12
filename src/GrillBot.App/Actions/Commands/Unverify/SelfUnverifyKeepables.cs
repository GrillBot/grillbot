using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using UnverifyService;
using UnverifyService.Models.Request.Keepables;

namespace GrillBot.App.Actions.Commands.Unverify;

public class SelfUnverifyKeepables(
    ITextsManager _texts,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : CommandAction
{
    public async Task<Embed> ListAsync(string? group = null)
    {
        var request = new KeepablesListRequest
        {
            Group = group,
            Pagination = new Core.Models.Pagination.PaginatedParams
            {
                Page = 0,
                PageSize = int.MaxValue
            }
        };

        var data = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetKeepablesListAsync(request, ctx.CancellationToken)
        );

        if (data.TotalItemsCount == 0)
            throw new NotFoundException(_texts["Unverify/SelfUnverify/Keepables/List/NoKeepables", Locale]);

        var groupedData = data.Data
            .GroupBy(o => o.Group.ToUpper())
            .ToDictionary(
                o => o.Key,
                o => o.Select(x => x.Name.ToUpper()).ToList()
            );

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .WithTitle(_texts["Unverify/SelfUnverify/Keepables/List/Title", Locale]);

        var otherGroupName = _texts["Unverify/SelfUnverify/Keepables/List/Other", Locale];
        foreach (var grp in groupedData.GroupBy(o => string.Join("|", o.Value)))
        {
            string fieldGroupResult;
            var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? otherGroupName : o.Key));

            var fieldGroupBuilder = new StringBuilder();
            foreach (var item in grp.First().Value)
            {
                if (fieldGroupBuilder.Length + item.Length >= EmbedFieldBuilder.MaxFieldValueLength)
                {
                    fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                    embed.AddField(keys, fieldGroupResult.EndsWith(',') ? fieldGroupResult[..^1] : fieldGroupResult);
                    fieldGroupBuilder.Clear();
                }
                else
                {
                    fieldGroupBuilder.Append(item).Append(", ");
                }
            }

            if (fieldGroupBuilder.Length <= 0)
                continue;

            fieldGroupResult = fieldGroupBuilder.ToString().Trim();
            embed.AddField(keys, fieldGroupResult.EndsWith(',') ? fieldGroupResult[..^1] : fieldGroupResult);
            fieldGroupBuilder.Clear();
        }

        return embed.Build();
    }
}
