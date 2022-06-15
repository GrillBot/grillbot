using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;

namespace GrillBot.App.Services;

[Initializable]
public class BoosterService
{
    private IConfiguration Configuration { get; }
    private InitManager InitManager { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public BoosterService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder,
        IConfiguration configuration, InitManager initManager)
    {
        Configuration = configuration;
        InitManager = initManager;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.GuildMemberUpdated += (before, after) =>
        {
            if (!before.HasValue) return Task.CompletedTask;
            if (!InitManager.Get()) return Task.CompletedTask;

            return !before.Value.Roles.SequenceEqual(after.Roles) ? OnGuildMemberUpdatedAsync(before.Value, after) : Task.CompletedTask;
        };
    }

    private async Task OnGuildMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.FindGuildAsync(before.Guild, true);
        if (guild?.BoosterRoleId == null || guild.AdminChannelId == null) return;

        var boostRole = before.Guild.GetRole(guild.BoosterRoleId.ToUlong());
        if (boostRole == null) return;

        var adminChannel = before.Guild.GetTextChannel(guild.AdminChannelId.ToUlong());
        if (adminChannel == null) return;

        var boostBefore = before.Roles.Any(o => o.Id.ToString() == guild.BoosterRoleId);
        var boostAfter = after.Roles.Any(o => o.Id.ToString() == guild.BoosterRoleId);
        if (!IsBoostReallyChanged(boostBefore, boostAfter)) return;

        var embed = new EmbedBuilder()
            .WithColor(boostRole.Color)
            .WithCurrentTimestamp()
            .AddField("Uživatel", after.GetFullName())
            .WithThumbnailUrl(after.GetUserAvatarUrl());

        switch (boostBefore)
        {
            case false when boostAfter:
                embed.WithTitle($"Uživatel je nyní Server Booster {Configuration["Discord:Emotes:Hypers"]}");
                break;
            case true when !boostAfter:
                embed.WithTitle($"Uživatel již není Server Booster {Configuration["Discord:Emotes:Sadge"]}");
                break;
        }

        await adminChannel.SendMessageAsync(embed: embed.Build());
    }

    private static bool IsBoostReallyChanged(bool before, bool after)
    {
        switch (before)
        {
            case true when after:
            case false when !after:
                return false;
            default:
                return true;
        }
    }
}
