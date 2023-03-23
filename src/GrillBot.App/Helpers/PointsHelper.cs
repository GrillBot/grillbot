using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Random;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using ChannelInfo = GrillBot.Common.Services.PointsService.Models.ChannelInfo;

namespace GrillBot.App.Helpers;

public class PointsHelper
{
    private IConfiguration Configuration { get; }
    private IDiscordClient DiscordClient { get; }
    private IRandomManager Random { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsHelper(IConfiguration configuration, IDiscordClient discordClient, IRandomManager random, IPointsServiceClient pointsServiceClient, GrillBotDatabaseBuilder databaseBuilder)
    {
        Configuration = configuration;
        DiscordClient = discordClient;
        Random = random;
        PointsServiceClient = pointsServiceClient;
        DatabaseBuilder = databaseBuilder;
    }

    public bool CanIncrementPoints(IMessage? message) 
        => message != null && message.Author.IsUser() && !message.IsCommand(DiscordClient.CurrentUser);

    public PointsTransaction? CreateTransaction(GuildUser user, string? reactionId, ulong messageId, bool ignoreCooldown)
    {
        var isReaction = !string.IsNullOrEmpty(reactionId);
        var cooldown = Configuration.GetValue<int>($"Points:Cooldown:{(isReaction ? "Reaction" : "Message")}");
        var range = Configuration.GetSection($"Points:Range:{(isReaction ? "Reaction" : "Message")}");

        var lastIncrement = isReaction ? user.LastPointsReactionIncrement : user.LastPointsMessageIncrement;
        if (!ignoreCooldown && lastIncrement.HasValue && lastIncrement.Value.AddSeconds(cooldown) > DateTime.Now)
            return null;

        var transaction = new PointsTransaction
        {
            GuildId = user.GuildId,
            Points = Random.GetNext("Points", range.GetValue<int>("From"), range.GetValue<int>("To")),
            AssingnedAt = DateTime.Now,
            ReactionId = reactionId ?? "",
            MessageId = messageId > 0 ? messageId.ToString() : SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            UserId = user.UserId
        };

        if (ignoreCooldown || messageId == 0)
            return transaction;

        if (isReaction)
            user.LastPointsReactionIncrement = transaction.AssingnedAt;
        else
            user.LastPointsMessageIncrement = transaction.AssingnedAt;
        return transaction;
    }

    public static async Task<bool> CanStoreTransactionAsync(GrillBotRepository repository, PointsTransaction? transaction)
        => transaction is { Points: > 0 } && !await repository.Points.ExistsTransactionAsync(transaction);

    public static async Task<List<PointsTransaction>> FilterTransactionsAsync(GrillBotRepository repository, params PointsTransaction?[] transactions)
    {
        var result = await transactions
            .FindAllAsync(async o => await CanStoreTransactionAsync(repository, o));

        return result.ConvertAll(o => o!);
    }

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
