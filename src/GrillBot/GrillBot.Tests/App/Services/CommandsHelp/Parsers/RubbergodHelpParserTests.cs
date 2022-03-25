using GrillBot.App.Services.CommandsHelp.Parsers;
using Newtonsoft.Json.Linq;

namespace GrillBot.Tests.App.Services.CommandsHelp.Parsers;

[TestClass]
public class RubbergodHelpParserTests
{
    private static JArray BuildHelp()
    {
        const string json = "[{\"commands\": [{\"command\": \"?uptime\",\"signature\": \"\",\"description\": \"Vyp\\u00ed\\u0161e \\u010das spu\\u0161t\\u011bn\\u00ed a \\u010das uplynul\\u00fd od spu\\u0161t\\u011bn\\u00ed\",\"aliases\": []}],\"description\": \"\",\"groupName\": \"Base\"},{\"commands\": [{\"command\": \"?karma\",\"signature\": \" \",\"description\": \"Vyp\\u00ed\\u0161e stav va\\u0161\\u00ed karmy (v\\u010d. rozdan\\u00e9 a odebran\\u00e9)\",\"aliases\": []},{\"command\": \"?karma get\",\"signature\": \"[args...]\",\"description\": \"Vr\\u00e1t\\u00ed karma hodnotu emotu\",\"aliases\": []},{\"command\": \"?karma message\",\"signature\": \"<message>\",\"description\": \"Zobraz\\u00ed karmu za zpr\\u00e1vu\",\"aliases\": []},{\"command\": \"?karma stalk\",\"signature\": \"<user>\",\"description\": \"Vyp\\u00ed\\u0161e karmu u\\u017eivatele\",\"aliases\": []},{\"command\": \"?karma getall\",\"signature\": \"\",\"description\": \"Vyp\\u00ed\\u0161e, kter\\u00e9 emoty maj\\u00ed hodnotu 1 a -1\",\"aliases\": []},{\"command\": \"?leaderboard\",\"signature\": \"[start=1]\",\"description\": \"Karma leaderboard\",\"aliases\": []},{\"command\": \"?bajkarboard\",\"signature\": \"[start=1]\",\"description\": \"Karma leaderboard reversed\",\"aliases\": []},{\"command\": \"?givingboard\",\"signature\": \"[start=1]\",\"description\": \"Leaderboard rozd\\u00e1v\\u00e1n\\u00ed pozitivn\\u00ed karmy\",\"aliases\": []},{\"command\": \"?ishaboard\",\"signature\": \"[start=1]\",\"description\": \"Leaderboard rozd\\u00e1v\\u00e1n\\u00ed negativn\\u00ed karmy\",\"aliases\": []}],\"description\": \"\",\"groupName\": \"Karma\"},{\"commands\": [{\"command\": \"?diceroll\",\"signature\": \"[arg]\",\"description\": \"V\\u0161echno mo\\u017en\\u00e9 h\\u00e1zen\\u00ed kostkami\",\"aliases\": []},{\"command\": \"?pick\",\"signature\": \"*Is foo bar? Yes No Maybe*\",\"description\": \"Vybere jedno ze slov za otazn\\u00edkem\",\"aliases\": []},{\"command\": \"?flip\",\"signature\": \"\",\"description\": \"Hod\\u00ed minc\\u00ed\",\"aliases\": []},{\"command\": \"?roll\",\"signature\": \"<first> [second=0]\",\"description\": \"Vygeneruje n\\u00e1hodn\\u00e9 cel\\u00e9 \\u010d\\u00edslo z intervalu <**first**, **second**>\",\"aliases\": [\"random\",\"randint\"]}],\"description\": \"\",\"groupName\": \"Random\"}]";

        return JArray.Parse(json);
    }

    [TestMethod]
    public void Parse()
    {
        var json = BuildHelp();
        var parser = new RubbergodHelpParser();
        var result = parser.Parse(json);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual(1, result[0].Commands.Count);
    }
}
