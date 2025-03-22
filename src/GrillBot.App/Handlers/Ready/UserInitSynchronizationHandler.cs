using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.Ready;

public class UserInitSynchronizationHandler : IReadyEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public UserInitSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync()
    {
        await ProcessMemberSynchronizationAsync();
        await ProcessBotAdminAsync();
    }

    private async Task ProcessMemberSynchronizationAsync()
    {
        var guilds = await DiscordClient.GetGuildsAsync();

        using var repository = DatabaseBuilder.CreateRepository();
        var users = await repository.GuildUser.GetAllUsersAsync();

        foreach (var guild in guilds)
        {
            var members = (await guild.GetUsersAsync()).ToDictionary(o => o.Id, o => o);

            foreach (var user in users.Where(o => o.GuildId == guild.Id.ToString()))
            {
                user.User!.Status = UserStatus.Offline;
                user.IsInGuild = false;

                if (!members.TryGetValue(user.UserId.ToUlong(), out var guildUser))
                    continue;

                user.Update(guildUser);
            }
        }

        await repository.CommitAsync();
    }

    private async Task ProcessBotAdminAsync()
    {
        var appInfo = await DiscordClient.GetApplicationInfoAsync();
        using var repository = DatabaseBuilder.CreateRepository();

        var botOwner = await repository.User.GetOrCreateUserAsync(appInfo.Owner);
        botOwner.Flags |= (int)UserFlags.BotAdmin;
        botOwner.Flags &= ~(int)UserFlags.NotUser;

        await repository.CommitAsync();
    }
}
