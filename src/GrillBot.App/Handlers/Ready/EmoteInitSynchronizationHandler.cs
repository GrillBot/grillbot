using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using Npgsql;

namespace GrillBot.App.Handlers.Ready;

public class EmoteInitSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteInitSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync()
    {
        //return;
        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = await repository.Emote.GetAllStatisticsAsync();
        const string CMD = "INSERT INTO public.\"EmoteUserStatItems\" (\"EmoteId\", \"EmoteName\", \"EmoteIsAnimated\", \"GuildId\", \"UserId\", \"UseCount\", \"FirstOccurence\", \"LastOccurence\") " +
            "VALUES (@emoteId, @emoteName, @emoteIsAnimated, @guildId, @userId, @useCount, @firstOccurence, @lastOccurence)";

        await using var connection = new NpgsqlConnection("Host=127.0.0.1;Database=EmoteService;Username=postgres;Password=Ngyv8S4#@Tylwv0x5(A80jiE8ULR2");
        await connection.OpenAsync();

        foreach (var stats in statistics)
        {
            await using var command = new NpgsqlCommand(CMD, connection);

            var emote = Emote.Parse(stats.EmoteId);
            var firstOccurence = (stats.FirstOccurence == DateTime.MinValue ? emote.CreatedAt.LocalDateTime : stats.FirstOccurence).WithKind(DateTimeKind.Local).ToUniversalTime();
            var lastOccurence = stats.LastOccurence.WithKind(DateTimeKind.Local).ToUniversalTime();

            command.Parameters.AddWithValue("@emoteId", emote.Id.ToString());
            command.Parameters.AddWithValue("@emoteName", emote.Name);
            command.Parameters.AddWithValue("@emoteIsAnimated", emote.Animated);
            command.Parameters.AddWithValue("@guildId", stats.GuildId);
            command.Parameters.AddWithValue("@userId", stats.UserId);
            command.Parameters.AddWithValue("@useCount", stats.UseCount);
            command.Parameters.AddWithValue("@firstOccurence", firstOccurence);
            command.Parameters.AddWithValue("@lastOccurence", lastOccurence);

            try
            {
                Console.WriteLine("Writing");
                await command.ExecuteNonQueryAsync();
            }
            catch (NpgsqlException ex) when (ex.Message.Contains("PK_EmoteUserStatItems"))
            {
                // Ignore duplicates
            }
        }
    }
}
