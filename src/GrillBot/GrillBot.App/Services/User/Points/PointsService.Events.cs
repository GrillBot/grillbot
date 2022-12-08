using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    private async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!channel.HasValue || channel.Value is not ITextChannel textChannel) return; // Only guilds
        if (reaction.Emote is not Emote || !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return; // Only local emotes.

        var user = (reaction.User.IsSpecified ? reaction.User.Value : await textChannel.Guild.GetUserAsync(reaction.UserId)) as IGuildUser;
        if (user?.IsUser() != true) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, textChannel);
        if (!CanIncrement(msg)) return;
        if (msg!.Author.Id == reaction.UserId) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(user.Guild);
        var userEntity = await repository.User.GetOrCreateUserAsync(user);
        var guildUser = await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        var channelEntity = await repository.Channel.GetOrCreateChannelAsync(textChannel);

        var userPointsDeactivated = (guildUser.User ?? userEntity).HaveFlags(UserFlags.PointsDisabled);
        if (channelEntity.HasFlag(ChannelFlags.PointsDeactivated) || userPointsDeactivated) return;

        var reactionId = CreateReactionId(reaction);
        var transaction = CreateTransaction(guildUser, reactionId, msg.Id, false);
        if (transaction == null || transaction.Points == 0) return;

        var migrated = CreateMigratedTransaction(guildUser, transaction);
        if (migrated != null && !await repository.Points.ExistsTransactionAsync(migrated))
            await repository.AddAsync(migrated);

        if (!await repository.Points.ExistsTransactionAsync(transaction))
            await repository.AddAsync(transaction);
        await repository.CommitAsync();
        await RecalculatePointsSummaryAsync(repository, true, new List<IGuildUser> { user });
    }

    private async Task OnReactionRemovedAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!channel.HasValue || channel.Value is not IGuildChannel guildChannel) return;
        if (reaction.Emote is not Emote || !guildChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return;

        var user = reaction.User.IsSpecified ? reaction.User.Value as IGuildUser : null;
        user ??= await guildChannel.Guild.GetUserAsync(reaction.UserId);

        await using var repository = DatabaseBuilder.CreateRepository();

        var reactionId = CreateReactionId(reaction);
        var transaction = await repository.Points.FindTransactionAsync(guildChannel.Guild, message.Id, reactionId, user);
        if (transaction == null) return;

        repository.Remove(transaction);
        await repository.CommitAsync();

        var onlyToday = transaction.AssingnedAt.Date == DateTime.Now.Date;
        await RecalculatePointsSummaryAsync(repository, onlyToday, new List<IGuildUser> { user });
    }

    private static string CreateReactionId(SocketReaction reaction)
        => $"{reaction.Emote}_{reaction.UserId}";
}
