using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Help;

public class CommandGroup
{
    public string GroupName { get; set; }
    public string Description { get; set; }
    public List<TextBasedCommand> Commands { get; set; }

    public CommandGroup()
    {
        Commands = new List<TextBasedCommand>();
    }
}
