using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Data.Models.API.Guilds.GuildEvents;

namespace GrillBot.App.Actions.Api.V2.Events;

public class UpdateScheduledEvent : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public UpdateScheduledEvent(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task ProcessAsync(ulong guildId, ulong eventId, ScheduledEventParams parameters)
    {
        var guildEvent = await FindAndCheckEventAsync(guildId, eventId);
        var image = CreateImage(parameters.Image);

        try
        {
            await guildEvent.ModifyAsync(prop => SetModificationParameters(prop, parameters, image, guildEvent));
        }
        finally
        {
            image?.Dispose();
        }
    }

    private async Task<IGuildScheduledEvent> FindAndCheckEventAsync(ulong guildId, ulong eventId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId)
            ?? throw new NotFoundException(Texts["GuildScheduledEvents/GuildNotFound", ApiContext.Language]);

        var guildEvent = await guild.GetEventAsync(eventId)
            ?? throw new NotFoundException(Texts["GuildScheduledEvents/EventNotFound", ApiContext.Language]);

        if (guildEvent.Creator.Id != DiscordClient.CurrentUser.Id)
            throw new ForbiddenAccessException(Texts["GuildScheduledEvents/ForbiddenAccess", ApiContext.Language]);
        return guildEvent;
    }

    private static Image? CreateImage(byte[]? image)
        => image == null || image.Length == 0 ? null : new Image(new MemoryStream(image));

    private static void SetModificationParameters(GuildScheduledEventsProperties properties, ScheduledEventParams parameters, Image? image, IGuildScheduledEvent currentEvent)
    {
        properties.ChannelId = null;
        properties.Type = GuildScheduledEventType.External;
        properties.PrivacyLevel = GuildScheduledEventPrivacyLevel.Private;
        properties.Location = !string.IsNullOrEmpty(parameters.Location) ? parameters.Location : currentEvent.Location;

        if (!string.IsNullOrEmpty(parameters.Name))
            properties.Name = parameters.Name;

        if (parameters.StartAt != DateTime.MinValue)
        {
            properties.StartTime = new DateTimeOffset(parameters.StartAt);
            properties.Status = parameters.StartAt >= DateTime.Now ? GuildScheduledEventStatus.Active : GuildScheduledEventStatus.Scheduled;
        }

        if (parameters.EndAt != DateTime.MinValue)
        {
            properties.EndTime = new DateTimeOffset(parameters.EndAt);

            if (parameters.EndAt < DateTime.Now)
                properties.Status = GuildScheduledEventStatus.Completed;
        }

        if (!string.IsNullOrEmpty(parameters.Description))
            properties.Description = parameters.Description;

        if (image != null)
            properties.CoverImage = image;
    }
}
