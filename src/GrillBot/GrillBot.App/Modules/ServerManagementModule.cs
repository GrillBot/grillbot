using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.Preconditions;
using GrillBot.Data;
using GrillBot.Database.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze provést jen na serveru.")]
    public class ServerManagementModule : Infrastructure.ModuleBase
    {
        private IConfiguration Configuration { get; }

        public ServerManagementModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("clean")]
        [Summary("Smaže zprávy v příslušném kanálu. Pokud nebyl zadán kanál jako parametr, tak bude použit kanál, kde byl zavolán příkaz.")]
        [RequireBotPermission(GuildPermission.ManageMessages, ErrorMessage = "Nemohu mazat zprávy, protože nemám oprávnění na mazání zpráv.")]
        [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidat reakce indikující stav.")]
        [RequireBotPermission(GuildPermission.ReadMessageHistory, ErrorMessage = "Nemohu mazat zprávy, protože nemám oprávnění na čtení historie.")]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "Tento příkaz může použít pouze uživatel, který má oprávnění mazat zprávy.")]
        public async Task CleanAsync([Name("pocet")] int take, [Name("kanal")] ITextChannel channel = null)
        {
            await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

            if (channel == null)
            {
                channel = Context.Channel as ITextChannel;
                take++;
            }

            var options = new RequestOptions()
            {
                AuditLogReason = $"Clean command from GrillBot. Executed {Context.User} in #{channel.Name}",
                RetryMode = RetryMode.AlwaysRetry,
                Timeout = 30000
            };

            var messages = (await channel.GetMessagesAsync(take, options: options).FlattenAsync())
                .Where(o => o.Id != Context.Message.Id);

            var older = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays >= 14.0);
            var newer = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays < 14.0);

            await channel.DeleteMessagesAsync(newer, options);

            foreach (var msg in older)
            {
                await msg.DeleteAsync(options);
            }

            await ReplyAsync($"Bylo úspěšně smazáno zpráv: **{messages.Count()}**\nStarších, než 2 týdny: **{older.Count()}**\nNovějších, než 2 týdny: **{newer.Count()}**");
            await Context.Message.RemoveAllReactionsAsync();
            await Context.Message.AddReactionAsync(Emojis.Ok);
        }

        [Group("pin")]
        [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "Nemohu provádet odepnutí zpráv, protože nemám oprávnění pracovat se zprávami.")]
        [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění přidat reakce indikující stav.")]
        [RequireBotPermission(GuildPermission.ReadMessageHistory, ErrorMessage = "Nemohu mazat zprávy, protože nemám oprávnění na čtení historie.")]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "Tento příkaz může použít pouze uživatel, který má oprávnění mazat zprávy.")]
        public class PinManagementSubmodule : Infrastructure.ModuleBase
        {
            private IConfiguration Configuration { get; }

            public PinManagementSubmodule(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            [Command("purge")]
            [Summary("Odepne zprávy z kanálu.")]
            public async Task PurgePinsAsync(ITextChannel channel = null, params ulong[] messageIds)
            {
                await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                if (channel == null)
                    channel = Context.Channel as ITextChannel;

                uint unpinned = 0;
                uint unknown = 0;
                uint notPinned = 0;

                foreach (var id in messageIds)
                {
                    if (await channel.GetMessageAsync(id) is not IUserMessage message)
                    {
                        unknown++;
                        continue;
                    }

                    if (!message.IsPinned)
                    {
                        notPinned++;
                        continue;
                    }

                    await message.UnpinAsync();
                    unpinned++;
                }

                await ReplyAsync($"Zprávy byly úspěšně odepnuty.\nCelkem zpráv: **{messageIds.Length}**\nOdepnutých: **{unpinned}**\nNepřipnutých: **{notPinned}**\nNeexistujících: **{unknown}**");

                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(Emojis.Ok);
            }

            [Command("purge")]
            [Summary("Odepne z kanálu určitý počet zpráv.")]
            public async Task PurgePinsAsync(int count, ITextChannel channel = null)
            {
                await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                if (channel == null)
                    channel = Context.Channel as ITextChannel;

                var pins = await channel.GetPinnedMessagesAsync();
                count = Math.Min(pins.Count, count);

                var pinCandidates = pins.Take(count).OfType<IUserMessage>().ToList();
                foreach (var pin in pinCandidates)
                {
                    await pin.UnpinAsync();
                }

                await ReplyAsync($"Zprávy byly úspěšně odepnuty.\nCelkem připnutých zpráv: **{pins.Count}**\nOdepnutých: **{pinCandidates.Count}**");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(Emojis.Ok);
            }
        }

        [Group("guild")]
        public class GuildManagementSubmodule : Infrastructure.ModuleBase
        {
            [Group("info")]
            [RequireBotPermission(GuildPermission.Administrator, ErrorMessage = "Nemohu provést tento příkaz, protože nemám nejvyšší oprávnění.")]
            [RequireUserPermission(GuildPermission.ViewGuildInsights, ErrorMessage = "Tento příkaz může provést pouze uživatel, který vidí statistiky serveru ve vývojářském portálu.")]
            public class GuildInfoSubModule : Infrastructure.ModuleBase
            {
                private GrillBotContextFactory DbContextFactory { get; }

                public GuildInfoSubModule(GrillBotContextFactory contextFactory)
                {
                    DbContextFactory = contextFactory;
                }

                [Command]
                [Summary("Informace o serveru, kde byl příkaz zavolán.")]
                public async Task InfoAsync()
                {
                    var guild = Context.Guild;

                    var basicEmotesCount = guild.Emotes.Count(o => !o.Animated);
                    var animatedCount = guild.Emotes.Count - basicEmotesCount;
                    var banCount = (await guild.GetBansAsync()).Count;
                    var tier = guild.PremiumTier.ToString().ToLower().Replace("tier", "").Replace("none", "0");

                    var color = guild.GetHighestRole(true)?.Color ?? Color.Default;
                    var embed = new EmbedBuilder()
                        .WithFooter(Context.User.GetDisplayName(), Context.User.GetAvatarUri())
                        .WithColor(color)
                        .WithTitle(guild.Name)
                        .WithThumbnailUrl(guild.IconUrl)
                        .WithCurrentTimestamp();

                    if (!string.IsNullOrEmpty(guild.Description))
                        embed.WithDescription(guild.Description[EmbedBuilder.MaxDescriptionLength..]);

                    if (!string.IsNullOrEmpty(guild.BannerId))
                        embed.WithImageUrl(guild.BannerUrl);

                    embed.AddField("Počet kategorií", guild.CategoryChannels?.Count ?? 0, true)
                        .AddField("Počet textových kanálů", guild.TextChannels.Count, true)
                        .AddField("Počet hlasových kanálů", guild.VoiceChannels.Count, true)
                        .AddField("Počet rolí", guild.Roles.Count, true)
                        .AddField("Počet emotů (běžných/animovaných)", $"{basicEmotesCount} / {animatedCount}", true)
                        .AddField("Počet banů", banCount, true)
                        .AddField("Vytvořen", guild.CreatedAt.LocalDateTime.ToCzechFormat(), true)
                        .AddField("Vlastník", guild.Owner.GetFullName(), false)
                        .AddField("Počet členů", guild.Users.Count, true)
                        .AddField("Úroveň serveru", tier, true)
                        .AddField("Počet boosterů", guild.PremiumSubscriptionCount, true);

                    if (guild.Features?.Count > 0)
                        embed.AddField("Vylepšení", string.Join("\n", guild.GetTranslatedFeatures()), false);

                    await ReplyAsync(embed: embed.Build());
                }

                [Command("status")]
                [Summary("Aktuální stav serveru (online uživatelé)")]
                public async Task StatusAsync()
                {
                    var guild = Context.Guild;
                    await guild.DownloadUsersAsync();

                    var groups = guild.Users.GroupBy(o => o.Status)
                        .ToDictionary(o => o.Key, o => o.Count());

                    var builder = new StringBuilder()
                        .Append("Server: **").Append(guild.Name).AppendLine("**")
                        .Append("Online: **").Append(groups.GetValueOrDefault(UserStatus.Online)).AppendLine("**")
                        .Append("Neaktivní: **").Append(groups.GetValueOrDefault(UserStatus.AFK) + groups.GetValueOrDefault(UserStatus.Idle)).AppendLine("**")
                        .Append("Nerušit: **").Append(groups.GetValueOrDefault(UserStatus.DoNotDisturb)).AppendLine("**")
                        .Append("Offline: **").Append(groups.GetValueOrDefault(UserStatus.Invisible) + groups.GetValueOrDefault(UserStatus.Offline)).AppendLine("**").AppendLine();

                    using var dbContext = DbContextFactory.Create();

                    var counts = dbContext.Guilds.AsQueryable().Where(o => o.Id == guild.Id.ToString()).Select(g => new
                    {
                        Users = g.Users.Count,
                        Invites = g.Invites.Count,
                        Channels = g.Channels.Count,
                        Searches = g.Searches.Count,
                        Unverifies = g.Unverifies.Count,
                        UnverifyLogs = g.UnverifyLogs.Count
                    });

                    var guildEntity = counts.FirstOrDefault();

                    if (guildEntity == null)
                    {
                        builder.AppendLine("**Pro tento server ještě v databázi neexistují žádné záznamy.**");
                    }
                    else
                    {
                        builder.AppendLine("Stav databáze:")
                            .Append("Uživatelů: **").Append(guildEntity.Users).AppendLine("**")
                            .Append("Pozvánek: **").Append(guildEntity.Invites).AppendLine("**")
                            .Append("Kanálů: **").Append(guildEntity.Channels).AppendLine("**")
                            .Append("Hledání: **").Append(guildEntity.Searches).AppendLine("**")
                            .Append("Unverify: **").Append(guildEntity.Unverifies).AppendLine("**")
                            .Append("Unverify (logy): **").Append(guildEntity.UnverifyLogs).AppendLine("**");
                    }

                    await ReplyAsync(builder.ToString());
                }

                [Command("limits")]
                [Summary("Zjistí limitní hranice serveru")]
                public async Task LimitsAsync()
                {
                    var guild = Context.Guild;

                    var builder = new StringBuilder()
                        .Append("Server: **").Append(guild.Name).AppendLine("**")
                        .Append("Maximum uživatelů: **").Append(guild.MaxMembers?.ToString() ?? "Není známo").AppendLine("**")
                        .Append("Maximum online uživatelů: **").Append(guild.MaxPresences?.ToString() ?? "Není známo").AppendLine("**")
                        .Append("Maximum uživatelů s webkamerou: **").Append(guild.MaxVideoChannelUsers?.ToString() ?? "Není známo").AppendLine("**")
                        .Append("Maximální bitrate: **").Append(guild.MaxBitrate / 1000).AppendLine(" kbps**")
                        .Append("Maximální upload: **").Append(guild.CalculateFileUploadLimit()).AppendLine(" MB**");

                    await ReplyAsync(builder.ToString());
                }
            }
        }
    }
}
