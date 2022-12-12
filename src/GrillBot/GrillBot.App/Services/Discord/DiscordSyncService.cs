using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Common.Managers;

namespace GrillBot.App.Services.Discord;

[Initializable]
public class DiscordSyncService
{
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private GuildSynchronization Guilds { get; }

    private bool MigrationsChecked { get; set; }

    public DiscordSyncService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, InitManager initManager)
    {
        InitManager = initManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        Guilds = new GuildSynchronization(DatabaseBuilder);

        DiscordClient.JoinedGuild += guild => RunAsync(() => Guilds.GuildAvailableAsync(guild));
        DiscordClient.GuildAvailable += guild => RunAsync(() => Guilds.GuildAvailableAsync(guild));
    }

    private async Task RunAsync(Func<Task> syncFunction, Func<bool> check = null)
    {
        if (!InitManager.Get()) return;
        if (check != null && !check()) return;

        if (!MigrationsChecked)
        {
            await using var repository = DatabaseBuilder.CreateRepository();
            await repository.ProcessMigrationsAsync();
            MigrationsChecked = true;
        }

        await syncFunction();
    }
}
