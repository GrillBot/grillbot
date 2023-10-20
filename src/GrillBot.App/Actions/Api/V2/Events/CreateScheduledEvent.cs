using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Data.Models.API.Guilds.GuildEvents;

namespace GrillBot.App.Actions.Api.V2.Events;

public class CreateScheduledEvent : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public CreateScheduledEvent(ApiRequestContext apiContext, IDiscordClient discordClient, ITextsManager texts) : base(apiContext)
    {
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task<ulong> ProcessAsync(ulong guildId, ScheduledEventParams parameters)
    {
        ValidateInput(parameters);
        var guild = await FindGuildAsync(guildId);
        var endTime = new DateTimeOffset(parameters.EndAt);
        var startTime = new DateTimeOffset(parameters.StartAt);
        var image = CreateImage(parameters.Image);

        try
        {
            var scheduledEvent = await guild.CreateEventAsync(parameters.Name, startTime, GuildScheduledEventType.External, GuildScheduledEventPrivacyLevel.Private, parameters.Description, endTime,
                null, parameters.Location, image);
            return scheduledEvent.Id;
        }
        finally
        {
            image?.Dispose();
        }
    }

    private async Task<IGuild> FindGuildAsync(ulong guildId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId);
        return guild ?? throw new NotFoundException(Texts["GuildScheduledEvents/GuildNotFound", ApiContext.Language]);
    }

    private static Image? CreateImage(byte[]? image)
        => image == null || image.Length == 0 ? null : new Image(new MemoryStream(image));

    private void ValidateInput(ScheduledEventParams parameters)
    {
        const string textsPrefix = "GuildScheduledEvents/Required/";

        if (string.IsNullOrEmpty(parameters.Name))
            throw new ValidationException(Texts[textsPrefix + "Name", ApiContext.Language]).ToBadRequestValidation(null, nameof(parameters.Name));
        if (parameters.StartAt == DateTime.MinValue)
            throw new ValidationException(Texts[textsPrefix + "StartAt", ApiContext.Language]).ToBadRequestValidation(DateTime.MinValue, nameof(parameters.StartAt));
        if (parameters.EndAt == DateTime.MinValue)
            throw new ValidationException(Texts[textsPrefix + "EndAt", ApiContext.Language]).ToBadRequestValidation(DateTime.MinValue, nameof(parameters.EndAt));
        if (string.IsNullOrEmpty(parameters.Location))
            throw new ValidationException(Texts[textsPrefix + "Location", ApiContext.Language]).ToBadRequestValidation(DateTime.MinValue, nameof(parameters.Location));
    }
}
