using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Services;

[Initializable]
public class BoosterService : ServiceBase
{
    private IConfiguration Configuration { get; }

    public BoosterService(DiscordSocketClient client, GrillBotContextFactory dbFactory,
        IConfiguration configuration, DiscordInitializationService initializationService) : base(client, dbFactory, initializationService)
    {
        Configuration = configuration;

        DiscordClient.GuildMemberUpdated += (before, after) =>
        {
            if (!before.HasValue) return Task.CompletedTask;
            if (!InitializationService.Get()) return Task.CompletedTask;

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
