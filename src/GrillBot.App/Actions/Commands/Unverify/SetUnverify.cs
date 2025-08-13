using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.Common.Exceptions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.GrillBot.Models;
using UnverifyService;
using UnverifyService.Models.Events;
using UnverifyService.Models.Request;

namespace GrillBot.App.Actions.Commands.Unverify;

public class SetUnverify(
    ITextsManager _texts,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient,
    IRabbitPublisher _rabbitPublisher,
    ICurrentUserProvider _currentUser
) : CommandAction
{

    // Command (Unverify)
    public async Task<List<string>> ProcessAsync(List<IGuildUser> users, DateTime end, string reason, bool testRun)
    {
        var messages = new List<string>();
        foreach (var user in users)
        {
            var message = await ProcessAsync(user, end, reason, false, [], testRun);
            messages.Add(message);
        }

        return messages;
    }

    // Command (Unverify + Selfunverify)
    public async Task<string> ProcessAsync(IUser user, DateTime end, string? reason, bool selfUnverify, List<string> toKeep, bool testRun)
    {
        var request = new UnverifyRequest
        {
            ChannelId = Context.Channel.Id,
            EndAtUtc = (end.Kind == DateTimeKind.Unspecified ? end.WithKind(DateTimeKind.Local) : end).ToUniversalTime(),
            GuildId = Context.Guild.Id,
            UserId = user.Id,
            IsSelfUnverify = selfUnverify,
            MessageId = Context.Interaction.Id,
            Reason = reason,
            RequiredKeepables = toKeep,
            TestRun = testRun
        };

        try
        {
            await _unverifyClient.ExecuteRequestAsync(
                async (client, ctx) => await client.CheckUnverifyRequirementsAsync(request, ctx.CancellationToken)
            );
        }
        catch (ClientBadRequestException ex) when (!string.IsNullOrEmpty(ex.RawData))
        {
            var validationError = JsonConvert.DeserializeObject<LocalizedMessageContent>(ex.RawData)!;
            var validationMessage = _texts[validationError, Locale];

            throw new ValidationException(validationMessage);
        }

        var setRequest = new SetUnverifyMessage(request);
        var headers = _currentUser.ToDictionary();

        await _rabbitPublisher.PublishAsync(setRequest, headers!);
        return _texts["Unverify/UnverifyStarted", Locale].FormatWith(user.GetDisplayName());
    }
}
