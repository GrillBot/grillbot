using GrillBot.Common.Models;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Common.Services.RubbergodService.Models.DirectApi;
using GrillBot.Data.Models.API.Help;

namespace GrillBot.App.Actions.Api.V1.Command;

public class GetExternalCommands : ApiAction
{
    private IRubbergodServiceClient RubbergodServiceClient { get; }

    public GetExternalCommands(ApiRequestContext apiContext, IRubbergodServiceClient rubbergodServiceClient) : base(apiContext)
    {
        RubbergodServiceClient = rubbergodServiceClient;
    }

    public async Task<List<CommandGroup>> ProcessAsync(string serviceName)
    {
        var command = new DirectApiCommand("Help")
            .WithParameter("user_id", ApiContext.GetUserId());

        var service = char.ToUpper(serviceName[0]) + serviceName[1..].ToLower();
        var jsonData = await RubbergodServiceClient.SendDirectApiCommand(service, command);
        var data = JArray.Parse(jsonData);

        return service switch
        {
            "Rubbergod" => RubbergodHelpParser.Parse(data),
            _ => new List<CommandGroup>()
        };
    }
}
