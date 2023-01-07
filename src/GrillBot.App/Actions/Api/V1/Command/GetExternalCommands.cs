using GrillBot.App.Services.CommandsHelp;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Help;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetExternalCommands : ApiAction
{
    private ExternalCommandsHelpService Service { get; }

    public GetExternalCommands(ApiRequestContext apiContext, ExternalCommandsHelpService service) : base(apiContext)
    {
        Service = service;
    }

    public async Task<List<CommandGroup>> ProcessAsync(string serviceName)
    {
        var userId = ApiContext.GetUserId();
        serviceName = char.ToUpper(serviceName[0]) + serviceName[1..].ToLower();

        return await Service.GetHelpAsync(serviceName, userId);
    }
}
