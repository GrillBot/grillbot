using GrillBot.App.Services.CommandsHelp.Parsers;
using GrillBot.App.Services.DirectApi;
using GrillBot.Data.Models.API.Help;
using System.Reflection;

namespace GrillBot.App.Services.CommandsHelp;

/// <summary>
/// Service for generating commands help for external bots.
/// </summary>
public class ExternalCommandsHelpService
{
    private IDirectApiService DirectApi { get; }
    private IConfiguration Configuration { get; }
    private IServiceProvider ServiceProvider { get; }
    private Type ParserInterfaceType { get; }

    public ExternalCommandsHelpService(IDirectApiService directApi, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        DirectApi = directApi;
        Configuration = configuration.GetSection("Services");
        ServiceProvider = serviceProvider;
        ParserInterfaceType = typeof(IHelpParser);
    }

    public async Task<List<CommandGroup>> GetHelpAsync(string service, ulong loggedUserId)
    {
        var configuration = Configuration.GetRequiredSection(service);
        var command = CommandBuilder.CreateHelpCommand(loggedUserId);
        var jsonData = await DirectApi.SendCommandAsync(service, command);
        var data = JArray.Parse(jsonData);

        return FindParserAndParse(configuration, data);
    }

    private List<CommandGroup> FindParserAndParse(IConfiguration externalServiceConfig, JArray json)
    {
        var parserName = externalServiceConfig.GetValue<string>("HelpParserClass");

        if (string.IsNullOrEmpty(parserName))
            return json.ToObject<List<CommandGroup>>();

        var parserType = Array.Find(
            Assembly.GetExecutingAssembly().GetTypes(),
            o => o.GetInterface(ParserInterfaceType.Name) != null && o.Name == parserName
        );

        if (parserType == null)
            return json.ToObject<List<CommandGroup>>();

        var parserInstance = ServiceProvider.GetService(parserType) as IHelpParser;
        if (parserInstance != null)
            return parserInstance.Parse(json);

        var constructor = parserType.GetConstructor(Type.EmptyTypes);
        if (constructor?.IsPublic == true && constructor.GetParameters().Length == 0)
            parserInstance = Activator.CreateInstance(parserType) as IHelpParser;

        // If no parser is defined or cannot get instance, then is used direct deserialization to API models.
        return parserInstance == null ? json.ToObject<List<CommandGroup>>() : parserInstance.Parse(json);
    }
}
