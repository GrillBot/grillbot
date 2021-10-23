using Discord;
using Discord.WebSocket;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class BoosterService : ServiceBase
    {
        private IConfiguration Configuration { get; }

        public BoosterService(DiscordSocketClient client, GrillBotContextFactory dbFactory,
            IConfiguration configuration) : base(client, dbFactory)
        {
            Configuration = configuration;

            DiscordClient.GuildMemberUpdated += (before, after) =>
            {
                if (!IsPremiumRoleChanged(before.Roles, after.Roles))
                    return OnGuildMemberUpdatedAsync(before, after);

                return Task.CompletedTask;
            };
        }

        private static bool IsPremiumRoleChanged(IReadOnlyCollection<SocketRole> before, IReadOnlyCollection<SocketRole> after)
        {
            if (before.SequenceEqual(after)) return false;

            var premiumRolesBefore = before.Where(o => o.Tags?.IsPremiumSubscriberRole == true).ToList();
            var premiumRolesAfter = after.Where(o => o.Tags?.IsPremiumSubscriberRole == true).ToList();
            return !premiumRolesBefore.SequenceEqual(premiumRolesAfter);
        }

        private async Task OnGuildMemberUpdatedAsync(SocketGuildUser before, SocketGuildUser after)
        {
            using var context = DbFactory.Create();

            var guild = await context.Guilds.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == before.Guild.Id.ToString());
            if (guild?.BoosterRoleId == null || guild?.AdminChannelId == null) return;

            var boostRole = before.Guild.GetRole(Convert.ToUInt64(guild.BoosterRoleId));
            if (boostRole == null) return;

            var adminChannel = before.Guild.GetTextChannel(Convert.ToUInt64(guild.AdminChannelId));
            if (adminChannel == null) return;

            var boostBefore = before.Roles.Any(o => o.Id.ToString() == guild.BoosterRoleId);
            var boostAfter = after.Roles.Any(o => o.Id.ToString() == guild.BoosterRoleId);

            var embed = new EmbedBuilder()
                .WithColor(boostRole.Color)
                .WithCurrentTimestamp()
                .AddField("Uživatel", after.GetFullName(), false)
                .WithThumbnailUrl(after.GetAvatarUri());

            if (!boostBefore && boostAfter)
                embed.WithTitle($"Uživatel je nyní Server Booster {Configuration["Discord:Emotes:Hypers"]}");
            else if (boostBefore && !boostAfter)
                embed.WithTitle($"Uživatel již není Server Booster {Configuration["Discord:Emotes:Sadge"]}");

            await adminChannel.SendMessageAsync(embed: embed.Build());
        }
    }
}
