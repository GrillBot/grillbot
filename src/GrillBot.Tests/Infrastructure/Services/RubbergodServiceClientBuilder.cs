using GrillBot.Common.Models.Pagination;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Common.Services.RubbergodService.Models.Diagnostics;
using GrillBot.Common.Services.RubbergodService.Models.DirectApi;
using GrillBot.Common.Services.RubbergodService.Models.Karma;
using Moq;

namespace GrillBot.Tests.Infrastructure.Services;

public class RubbergodServiceClientBuilder : BuilderBase<IRubbergodServiceClient>
{
    public RubbergodServiceClientBuilder SetAll()
        => SetIsAvailableAction().SetDiagAction().SetRefreshMemberAction().SetSendDirectApi().SetGetKarmaPageAction().SetStoreKarmaAction();

    public RubbergodServiceClientBuilder SetDiagAction()
    {
        Mock.Setup(o => o.GetDiagAsync()).ReturnsAsync(new DiagnosticInfo
        {
            Uptime = 5000,
            Version = "1.0.0.0",
            Endpoints = new List<RequestStatistic>(),
            MeasuredFrom = DateTime.Now,
            RequestsCount = 50,
            UsedMemory = 50
        });
        return this;
    }

    public RubbergodServiceClientBuilder SetIsAvailableAction()
    {
        Mock.Setup(o => o.IsAvailableAsync()).ReturnsAsync(true);
        return this;
    }

    public RubbergodServiceClientBuilder SetRefreshMemberAction()
    {
        Mock.Setup(o => o.RefreshMemberAsync(It.IsAny<ulong>())).Returns(Task.CompletedTask);
        return this;
    }

    public RubbergodServiceClientBuilder SetSendDirectApi()
    {
        Mock.Setup(o => o.SendDirectApiCommand(It.Is<string>(x => x == "Rubbergod"), It.Is<DirectApiCommand>(x => x.Method == "Help"))).ReturnsAsync(
            "[{\"commands\": [{\"command\": \"?uptime\",\"signature\": \"\",\"description\": \"Vyp\\u00ed\\u0161e \\u010das spu\\u0161t\\u011bn\\u00ed a \\u010das uplynul\\u00fd od spu\\u0161t\\u011bn\\u00ed\",\"aliases\": []}],\"description\": \"\",\"groupName\": \"Base\"},{\"commands\": [{\"command\": \"?karma\",\"signature\": \" \",\"description\": \"Vyp\\u00ed\\u0161e stav va\\u0161\\u00ed karmy (v\\u010d. rozdan\\u00e9 a odebran\\u00e9)\",\"aliases\": []},{\"command\": \"?karma get\",\"signature\": \"[args...]\",\"description\": \"Vr\\u00e1t\\u00ed karma hodnotu emotu\",\"aliases\": []},{\"command\": \"?karma message\",\"signature\": \"<message>\",\"description\": \"Zobraz\\u00ed karmu za zpr\\u00e1vu\",\"aliases\": []},{\"command\": \"?karma stalk\",\"signature\": \"<user>\",\"description\": \"Vyp\\u00ed\\u0161e karmu u\\u017eivatele\",\"aliases\": []},{\"command\": \"?karma getall\",\"signature\": \"\",\"description\": \"Vyp\\u00ed\\u0161e, kter\\u00e9 emoty maj\\u00ed hodnotu 1 a -1\",\"aliases\": []},{\"command\": \"?leaderboard\",\"signature\": \"[start=1]\",\"description\": \"Karma leaderboard\",\"aliases\": []},{\"command\": \"?bajkarboard\",\"signature\": \"[start=1]\",\"description\": \"Karma leaderboard reversed\",\"aliases\": []},{\"command\": \"?givingboard\",\"signature\": \"[start=1]\",\"description\": \"Leaderboard rozd\\u00e1v\\u00e1n\\u00ed pozitivn\\u00ed karmy\",\"aliases\": []},{\"command\": \"?ishaboard\",\"signature\": \"[start=1]\",\"description\": \"Leaderboard rozd\\u00e1v\\u00e1n\\u00ed negativn\\u00ed karmy\",\"aliases\": []}],\"description\": \"\",\"groupName\": \"Karma\"},{\"commands\": [{\"command\": \"?diceroll\",\"signature\": \"[arg]\",\"description\": \"V\\u0161echno mo\\u017en\\u00e9 h\\u00e1zen\\u00ed kostkami\",\"aliases\": []},{\"command\": \"?pick\",\"signature\": \"*Is foo bar? Yes No Maybe*\",\"description\": \"Vybere jedno ze slov za otazn\\u00edkem\",\"aliases\": []},{\"command\": \"?flip\",\"signature\": \"\",\"description\": \"Hod\\u00ed minc\\u00ed\",\"aliases\": []},{\"command\": \"?roll\",\"signature\": \"<first> [second=0]\",\"description\": \"Vygeneruje n\\u00e1hodn\\u00e9 cel\\u00e9 \\u010d\\u00edslo z intervalu <**first**, **second**>\",\"aliases\": [\"random\",\"randint\"]}],\"description\": \"\",\"groupName\": \"Random\"}]");
        Mock.Setup(o => o.SendDirectApiCommand(It.Is<string>(x => x == "Rubbergod-help-test"), It.IsAny<DirectApiCommand>())).ReturnsAsync("[]");
        Mock.Setup(o => o.SendDirectApiCommand(It.Is<string>(x => x == "Rubbergod"), It.Is<DirectApiCommand>(x => x.Method == "Help" && x.Parameters!.ContainsKey("command"))))
            .ReturnsAsync("{\"streamlinks\": {\"id\": 1037655021640233021, \"children\": [\"get\", \"list\"]}}");
        return this;
    }

    public RubbergodServiceClientBuilder SetGetKarmaPageAction()
    {
        Mock.Setup(o => o.GetKarmaPageAsync(It.IsAny<PaginatedParams>())).ReturnsAsync(new PaginatedResponse<UserKarma>()
        {
            Data = { new UserKarma() }
        });
        return this;
    }

    public RubbergodServiceClientBuilder SetStoreKarmaAction()
    {
        Mock.Setup(o => o.StoreKarmaAsync(It.IsAny<List<KarmaItem>>())).Returns(Task.CompletedTask);
        return this;
    }
}
