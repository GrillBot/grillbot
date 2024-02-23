using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.PointsService.Models.Events;
using GrillBot.Core.Services.PointsService.Models.Users;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using ChannelInfo = GrillBot.Core.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Helpers;

public class PointsHelper
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IRabbitMQPublisher RabbitPublisher { get; }

    public PointsHelper(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IRabbitMQPublisher rabbitPublisher)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        RabbitPublisher = rabbitPublisher;
    }

    public async Task<bool> CanIncrementPointsAsync(IMessage message, IUser? reactionUser = null)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var user = reactionUser ?? message.Author;
        if (!await IsValidForIncrementAsync(repository, user)) // IsBot, DisabledPoints
            return false;

        if (!await IsValidForIncrementAsync(repository, message.Channel)) // IsDeleted, DisabledPoints
            return false;

        if (message.IsCommand(DiscordClient.CurrentUser)) // CommandCheck
            return false;

        if (message.Author.Id == reactionUser?.Id) // SelfReaction
            return false;

        return true;
    }

    private static async Task<bool> IsValidForIncrementAsync(GrillBotRepository repository, IUser user)
    {
        if (!user.IsUser())
            return false;

        var entity = await repository.User.FindUserAsync(user, true);
        return entity?.HaveFlags(UserFlags.PointsDisabled) == false;
    }

    private static async Task<bool> IsValidForIncrementAsync(GrillBotRepository repository, IChannel channel)
    {
        if (channel is not ITextChannel textChannel)
            return false;

        var entity = await repository.Channel.FindChannelByIdAsync(textChannel.Id, textChannel.GuildId, true);
        return entity?.HasFlag(ChannelFlag.PointsDeactivated) == false;
    }

    public async Task<bool> IsUserAcceptableAsync(IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var userEntity = await repository.User.FindUserAsync(user, true);
        return userEntity?.HaveFlags(UserFlags.NotUser) == false && !userEntity.HaveFlags(UserFlags.PointsDisabled);
    }

    public Task PushSynchronizationAsync(IGuild guild, params IUser[] users) => PushSynchronizationAsync(guild, users, Enumerable.Empty<IGuildChannel>());
    public Task PushSynchronizationAsync(IGuildChannel channel) => PushSynchronizationAsync(channel.Guild, channel);
    public Task PushSynchronizationAsync(IGuild guild, params IGuildChannel[] channels) => PushSynchronizationAsync(guild, Enumerable.Empty<IUser>(), channels);

    public async Task PushSynchronizationAsync(IGuild guild, IEnumerable<IUser> users, IEnumerable<IGuildChannel> channels)
    {
        var channelInfos = new List<ChannelInfo>();
        var userInfos = new List<UserInfo>();

        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var user in users)
        {
            var entity = await repository.User.FindUserAsync(user, true);
            if (entity is null) continue;

            userInfos.Add(new UserInfo
            {
                PointsDisabled = entity.HaveFlags(UserFlags.PointsDisabled),
                Id = user.Id.ToString(),
                IsUser = user.IsUser()
            });
        }

        foreach (var channelId in channels.Select(o => o.Id))
        {
            var entity = await repository.Channel.FindChannelByIdAsync(channelId, guild.Id, true, includeDeleted: true);
            if (entity is null) continue;

            channelInfos.Add(new ChannelInfo
            {
                Id = channelId.ToString(),
                PointsDisabled = entity.HasFlag(ChannelFlag.PointsDeactivated),
                IsDeleted = entity.HasFlag(ChannelFlag.Deleted)
            });
        }

        var payload = new SynchronizationPayload(guild.Id.ToString(), channelInfos, userInfos);
        await PushPayloadAsync(payload);
    }

    public static bool IsMissingData(ValidationProblemDetails? details)
    {
        if (details is null) return false;

        var errors = details.Errors.SelectMany(o => o.Value).Distinct().ToList();
        return errors.Contains("UnknownChannel") || errors.Contains("UnknownUser");
    }

    public async Task PushPayloadAsync<TPayload>(TPayload payload)
    {
        // TODO Implement to GrillBot.Core packages
        var queueName = Array.Find(
            typeof(TPayload).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy),
            f => f.Name == "QueueName" && f.IsLiteral && !f.IsInitOnly
        )?.GetRawConstantValue() as string;

        if (string.IsNullOrEmpty(queueName))
            throw new InvalidOperationException("Unable to publish data to the queue without queue name");

        await RabbitPublisher.PublishAsync(queueName, payload);
    }
}
