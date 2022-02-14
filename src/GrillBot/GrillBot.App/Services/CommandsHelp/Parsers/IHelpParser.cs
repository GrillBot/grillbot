using GrillBot.Data.Models.API.Help;

namespace GrillBot.App.Services.CommandsHelp.Parsers;

public interface IHelpParser
{
    List<CommandGroup> Parse(JArray json);
}
