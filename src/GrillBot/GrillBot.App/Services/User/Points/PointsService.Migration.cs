using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    private List<ulong> UsersWithoutPoints { get; } = new();
    private List<ulong> TransactionIds { get; set; }

    public async Task ProcessMigrationAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        TransactionIds = await repository.Points.GetTransactionMessageIds();

        if (!await repository.GuildUser.ExistsUserWithOldPointsAsync())
            return;

        foreach (var guild in DiscordClient.Guilds)
        {
            await ProcessMigrationAsync(repository, guild);
        }

        await repository.CommitAsync();
    }

    private async Task ProcessMigrationAsync(GrillBotRepository repository, IGuild guild)
    {
        var channels = (await guild.GetTextChannelsAsync()).ToList();
        channels = channels
            .OrderByDescending(o => o.Name.Length)
            .ToList();

        foreach (var channel in channels)
        {
            await ProcessMigrationAsync(repository, guild, channel);
        }

        await repository.CommitAsync();
    }

    private async Task ProcessMigrationAsync(GrillBotRepository repository, IGuild guild, ITextChannel channel)
    {
        await foreach (var group in channel.GetMessagesAsync(int.MaxValue))
        {
            foreach (var message in group)
            {
                await ProcessMigrationAsync(repository, guild, channel, message);
            }
        }
    }

    private async Task ProcessMigrationAsync(GrillBotRepository repository, IGuild guild, ITextChannel channel, IMessage message)
    {
        if (UsersWithoutPoints.Contains(message.Author.Id)) return;
        if (TransactionIds.Contains(message.Id)) return;

        var author = await guild.GetUserAsync(message.Author.Id);
        if (author == null) return;

        var guildUser = await repository.GuildUser.FindGuildUserAsync(author);
        if (guildUser == null || guildUser.Points == 0)
        {
            UsersWithoutPoints.Add(author.Id);
            return;
        }

        var transaction = CreateTransaction(guildUser, false, message.Id, true);
        transaction.AssingnedAt = message.CreatedAt.LocalDateTime;
        guildUser.Points -= transaction.Points;
        if (guildUser.Points < 0) guildUser.Points = 0;

        await repository.AddAsync(transaction);
        await repository.CommitAsync();
    }
}
