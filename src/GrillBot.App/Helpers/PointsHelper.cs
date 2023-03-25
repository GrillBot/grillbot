using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using ChannelInfo = GrillBot.Common.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Helpers;

public class PointsHelper
{
    private IDiscordClient DiscordClient { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsHelper(IDiscordClient discordClient, IPointsServiceClient pointsServiceClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        DiscordClient = discordClient;
        PointsServiceClient = pointsServiceClient;
        DatabaseBuilder = databaseBuilder;
    }

    public bool CanIncrementPoints(IMessage? message)
        => message != null && message.Author.IsUser() && !message.IsCommand(DiscordClient.CurrentUser);

    public async Task SyncDataWithServiceAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels)
    {
        var request = new SynchronizationRequest
        {
            GuildId = guild.Id.ToString()
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var user in users)
        {
            request.Users.Add(new UserInfo
            {
                PointsDisabled = await repository.User.HaveDisabledPointsAsync(user),
                Id = user.Id.ToString(),
                IsUser = user.IsUser()
            });
        }

        foreach (var channel in channels)
        {
            request.Channels.Add(new ChannelInfo
            {
                Id = channel.Id.ToString(),
                PointsDisabled = await repository.Channel.HaveChannelFlagsAsync(channel, ChannelFlag.PointsDeactivated),
                IsDeleted = await repository.Channel.HaveChannelFlagsAsync(channel, ChannelFlag.Deleted)
            });
        }

        await PointsServiceClient.ProcessSynchronizationAsync(request);
    }

    public static bool CanSyncData(ValidationProblemDetails? details)
    {
        if (details is null) return false;

        var errors = details.Errors.SelectMany(o => o.Value).Distinct().ToList();
        return errors.Contains("UnknownChannel") || errors.Contains("UnknownUser");
    }
}
