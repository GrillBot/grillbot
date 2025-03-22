using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.PointsService.Models.Channels;
using GrillBot.Core.Services.PointsService.Models.Users;
using PointsModels = GrillBot.Core.Services.PointsService.Models;

namespace GrillBot.App.Managers.Points;

public class PointsSynchronizationManager
{
    private readonly IRabbitPublisher _rabbitPublisher;
    private readonly IDiscordClient _discordClient;

    public PointsSynchronizationManager(IRabbitPublisher rabbitPublisher, IDiscordClient discordClient)
    {
        _rabbitPublisher = rabbitPublisher;
        _discordClient = discordClient;
    }

    public async Task PushUsersAsync(IEnumerable<IUser> users)
    {
        var guilds = new Dictionary<IGuild, List<IUser>>();
        foreach (var user in users)
        {
            var mutualGuilds = await _discordClient.FindMutualGuildsAsync(user.Id);

            foreach (var guild in mutualGuilds)
            {
                if (!guilds.ContainsKey(guild))
                    guilds.Add(guild, new List<IUser>());
                guilds[guild].Add(user);
            }
        }

        foreach (var guild in guilds)
            await PushAsync(guild.Key, guild.Value, Enumerable.Empty<IGuildChannel>());
    }

    public Task PushAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels)
    {
        var channelList = channels.Select(o => new ChannelSyncItem { Id = o.Id.ToString() });

        var userList = users.Select(o => new UserSyncItem
        {
            Id = o.Id.ToString(),
            IsUser = o.IsUser()
        });

        return PushAsync(guild, userList, channelList);
    }

    public async Task PushAsync(IGuild guild, IEnumerable<UserSyncItem> users, IEnumerable<ChannelSyncItem> channels)
    {
        var payload = new PointsModels.Events.SynchronizationPayload(guild.Id.ToString(), channels.ToList(), users.ToList());
        await _rabbitPublisher.PublishAsync(payload, new());
    }
}
