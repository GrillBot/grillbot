using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageReceived;

public class PointsMessageReceivedHandler : IMessageReceivedEvent
{
    private PointsHelper Helper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsMessageReceivedHandler(PointsHelper helper, GrillBotDatabaseBuilder databaseBuilder)
    {
        Helper = helper;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!Helper.CanIncrementPoints(message) || message.Channel is not ITextChannel textChannel) return;
        var guildUser = message.Author as IGuildUser ?? await textChannel.Guild.GetUserAsync(message.Author.Id);

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(textChannel.Guild);
        var userEntity = await repository.User.GetOrCreateUserAsync(guildUser);
        var guildUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(guildUser);
        var guildChannel = await repository.Channel.FindChannelByIdAsync(textChannel.Id, textChannel.GuildId, true);
        if (!PointsHelper.CanIncrementPoints(userEntity, guildChannel)) return;

        var transaction = Helper.CreateTransaction(guildUserEntity, null, message.Id, false);
        if (!await PointsHelper.CanStoreTransactionAsync(repository, transaction)) return;

        await repository.AddAsync(transaction!);
        await repository.CommitAsync();
    }
}
