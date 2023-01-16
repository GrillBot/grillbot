using GrillBot.App.Services.DirectApi;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Help;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetExternalCommands : ApiAction
{
    private IDirectApiService DirectApi { get; }

    public GetExternalCommands(ApiRequestContext apiContext, IDirectApiService directApi) : base(apiContext)
    {
        DirectApi = directApi;
    }

    public async Task<List<CommandGroup>> ProcessAsync(string serviceName)
    {
        var command = CommandBuilder.CreateHelpCommand(ApiContext.GetUserId());
        var service = char.ToUpper(serviceName[0]) + serviceName[1..].ToLower();
        var jsonData = await DirectApi.SendCommandAsync(service, command);
        var data = JArray.Parse(jsonData ?? "[]");

        return service switch
        {
            "Rubbergod" => RubbergodHelpParser.Parse(data),
            _ => new List<CommandGroup>()
        };
    }
}
