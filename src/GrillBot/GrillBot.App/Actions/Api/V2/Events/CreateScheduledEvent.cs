using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Data.Exceptions;
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

    private static Image? CreateImage(byte[] image)
    {
        if (image == null || image.Length == 0) return null;

        return new Image(new MemoryStream(image));
    }
}
