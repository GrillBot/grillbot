using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Commands.Unverify;

public class SelfUnverifyKeepables : CommandAction
{
    private Api.V1.Unverify.GetKeepablesList ApiAction { get; }
    private ITextsManager Texts { get; }

    public SelfUnverifyKeepables(Api.V1.Unverify.GetKeepablesList apiAction, ITextsManager texts)
    {
        ApiAction = apiAction;
        Texts = texts;
    }

    public async Task<Embed> ListAsync(string? group = null)
    {
        ApiAction.UpdateContext(Locale, Context.User);

        var data = await ApiAction.ProcessAsync(group);
        if (data.Count == 0)
            throw new NotFoundException(Texts["Unverify/SelfUnverify/Keepables/List/NoKeepables", Locale]);

        var embed = new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(Context.User)
            .WithTitle(Texts["Unverify/SelfUnverify/Keepables/List/Title", Locale]);

        var otherGroupName = Texts["Unverify/SelfUnverify/Keepables/List/Other", Locale];
        foreach (var grp in data.GroupBy(o => string.Join("|", o.Value)))
        {
            string fieldGroupResult;
            var keys = string.Join(", ", grp.Select(o => o.Key == "_" ? otherGroupName : o.Key));

            var fieldGroupBuilder = new StringBuilder();
            foreach (var item in grp.First().Value)
            {
                if (fieldGroupBuilder.Length + item.Length >= EmbedFieldBuilder.MaxFieldValueLength)
                {
                    fieldGroupResult = fieldGroupBuilder.ToString().Trim();
                    embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[..^1] : fieldGroupResult);
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
            embed.AddField(keys, fieldGroupResult.EndsWith(",") ? fieldGroupResult[..^1] : fieldGroupResult);
            fieldGroupBuilder.Clear();
        }

        return embed.Build();
    }
}
