using GrillBot.Data.Models.DirectApi;

namespace GrillBot.App.Services.DirectApi;

public interface IDirectApiService
{
    Task<string?> SendCommandAsync(string service, DirectMessageCommand command);
}
