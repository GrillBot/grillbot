using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.Emote.Models.Events;
using GrillBot.Core.Services.Emote.Models.Events.Guild;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class EmoteOrchestrationHandler
{
    // Ready
    async Task IReadyEvent.ProcessAsync()
    {
        var guilds = await _discordClient.GetGuildsAsync();

        await ProcessSupportedEmotesSynchronization(guilds);
        await ProcessMissingChannelsAsync(guilds);
    }

    private async Task ProcessSupportedEmotesSynchronization(IReadOnlyCollection<IGuild> guilds)
    {
        var payloads = new List<SynchronizeEmotesPayload>();

        foreach (var guild in guilds)
        {
            var emotes = guild.Emotes.ToList();
            payloads.Add(new SynchronizeEmotesPayload(guild.Id.ToString(), emotes));
        }

        if (payloads.Count > 0)
            await _rabbitPublisher.PublishAsync(payloads);
    }

    private async Task ProcessMissingChannelsAsync(IReadOnlyCollection<IGuild> guilds)
    {
        var payloads = new List<GuildChannelDeletedPayload>();

        foreach (var guild in guilds)
        {
            try
            {
                var channels = await guild.GetChannelsAsync();
                var guildData = await _emoteService.ExecuteRequestAsync((c, ctx) => c.GetGuildAsync(guild.Id, ctx.CancellationToken));

                if (guildData.SuggestionChannelId is not null && !channels.Any(o => o.Id == guildData.SuggestionChannelId))
                    payloads.Add(new GuildChannelDeletedPayload(guild.Id, guildData.SuggestionChannelId.Value));

                if (guildData.VoteChannelId is not null && !channels.Any(o => o.Id == guildData.VoteChannelId))
                    payloads.Add(new GuildChannelDeletedPayload(guild.Id, guildData.VoteChannelId.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while processing deleted channels for emote suggestions. Guild: {Name}", guild.Name);
            }
        }

        if (payloads.Count > 0)
            await _rabbitPublisher.PublishAsync(payloads);
    }
}
