using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers;

namespace GrillBot.App.Services;

[Initializable]
public class BoosterService : ServiceBase
{
    private IConfiguration Configuration { get; }
    private InitManager InitManager { get; }

    public BoosterService(DiscordSocketClient client, GrillBotDatabaseFactory dbFactory,
        IConfiguration configuration, InitManager initManager) : base(client, dbFactory)
    {
        Configuration = configuration;
        InitManager = initManager;

        DiscordClient.GuildMemberUpdated += (before, after) =>
        {
            if (!before.HasValue) return Task.CompletedTask;
            if (!InitManager.Get()) return Task.CompletedTask;

            if (!before.Value.Roles.SequenceEqual(after.Roles))
                return OnGuildMemberUpdatedAsync(before.Value, after);

            return Task.CompletedTask;
        };
    }

    private async Task OnGuildMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
    {
        using var context = DbFactory.Create();

        var guild = await context.Guilds.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == before.Guild.Id.ToString());
        if (guild?.BoosterRoleId == null || guild?.AdminChannelId == null) return;

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
            .AddField("Uživatel", after.GetFullName(), false)
            .WithThumbnailUrl(after.GetUserAvatarUrl());

        if (!boostBefore && boostAfter)
            embed.WithTitle($"Uživatel je nyní Server Booster {Configuration["Discord:Emotes:Hypers"]}");
        else if (boostBefore && !boostAfter)
            embed.WithTitle($"Uživatel již není Server Booster {Configuration["Discord:Emotes:Sadge"]}");

        await adminChannel.SendMessageAsync(embed: embed.Build());
    }

    private static bool IsBoostReallyChanged(bool before, bool after)
    {
        if (before && after) return false;
        if (!before && !after) return false;

        return true;
    }
}
