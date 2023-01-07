using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Actions.Api.V2.Events;

public class CancelScheduledEvent : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public CancelScheduledEvent(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task ProcessAsync(ulong guildId, ulong eventId)
    {
        var @event = await FindAndCheckEventAsync(guildId, eventId);
        await @event.EndAsync();
    }

    private async Task<IGuildScheduledEvent> FindAndCheckEventAsync(ulong guildId, ulong eventId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        if (guild == null) throw new NotFoundException(Texts["GuildScheduledEvents/GuildNotFound", ApiContext.Language]);

        var guildEvent = await guild.GetEventAsync(eventId);
        ValidateCancelation(guildEvent);

        return guildEvent;
    }

    private void ValidateCancelation(IGuildScheduledEvent guildEvent)
    {
        if (guildEvent == null)
            throw new NotFoundException(Texts["GuildScheduledEvents/EventNotFound", ApiContext.Language]);
        if (guildEvent.Creator.Id != DiscordClient.CurrentUser.Id)
            throw new ForbiddenAccessException(Texts["GuildScheduledEvents/ForbiddenAccess", ApiContext.Language]);

        var canCancel = guildEvent.Status != GuildScheduledEventStatus.Completed && guildEvent.Status != GuildScheduledEventStatus.Cancelled && guildEvent.EndTime >= DateTime.Now;
        if (!canCancel)
            throw new ValidationException(Texts["GuildScheduledEvents/CannotCancelEventEnded", ApiContext.Language]).ToBadRequestValidation(guildEvent, nameof(guildEvent));
    }
}
