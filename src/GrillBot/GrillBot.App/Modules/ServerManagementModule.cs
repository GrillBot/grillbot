using ConsoleTableExt;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Preconditions;
using GrillBot.App.Services;
using GrillBot.Data;
using GrillBot.Data.Enums;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.Guilds;
using GrillBot.Database.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    [Name("Správa serveru")]
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
        [Name("Správa pinů")]
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
            public async Task PurgePinsAsync([Name("kanal")] ITextChannel channel = null, [Name("id_zprav")] params ulong[] messageIds)
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
            public async Task PurgePinsAsync([Name("pocet")] int count, [Name("kanal")] ITextChannel channel = null)
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
            [Name("Informace o serveru")]
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
                        .WithFooter(Context.User)
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

                    var groupsQuery = guild.Users.GroupBy(o => o.Status);
                    var groups = groupsQuery.ToDictionary(o => o.Key, o => o.Count());

                    var embed = new EmbedBuilder()
                        .WithAuthor("Stav serveru")
                        .WithColor(guild.GetHighestRole(true)?.Color ?? Color.Blue)
                        .WithCurrentTimestamp()
                        .WithFooter(Context.User)
                        .WithTitle(guild.Name)
                        .WithThumbnailUrl(guild.IconUrl)
                        .AddField("Online", groups.GetValueOrDefault(UserStatus.Online), true)
                        .AddField("Neaktivní", groups.GetValueOrDefault(UserStatus.AFK) + groups.GetValueOrDefault(UserStatus.Idle), true)
                        .AddField("Nerušit", groups.GetValueOrDefault(UserStatus.DoNotDisturb), true)
                        .AddField("Offline", groups.GetValueOrDefault(UserStatus.Invisible) + groups.GetValueOrDefault(UserStatus.Offline), true);

                    using var dbContext = DbContextFactory.Create();

                    var counts = dbContext.Guilds.AsQueryable().Where(o => o.Id == guild.Id.ToString()).Select(g => new
                    {
                        Users = g.Users.Count,
                        Invites = g.Invites.Count,
                        Channels = g.Channels.Count,
                        Searches = g.Searches.Count,
                        Unverifies = g.Unverifies.Count,
                        UnverifyLogs = g.UnverifyLogs.Count,
                        AuditLogs = g.AuditLogs.Count
                    });

                    var guildEntity = counts.FirstOrDefault();

                    if (guildEntity == null)
                    {
                        embed.AddField("Stav databáze", "Pro tento server ještě v databázi neexistují žádné záznamy.", false);
                    }
                    else
                    {
                        embed.AddField("Stav databáze", "__ __", false)
                            .AddField("Uživatelů", guildEntity.Users, true)
                            .AddField("Pozvánek", guildEntity.Invites, true)
                            .AddField("Kanálů", guildEntity.Channels, true)
                            .AddField("Hledání", guildEntity.Searches, true)
                            .AddField("Unverify", guildEntity.Unverifies, true)
                            .AddField("Unverify (logy)", guildEntity.UnverifyLogs, true)
                            .AddField("Logy", guildEntity.AuditLogs, true);
                    }

                    var devices = groupsQuery.Where(o => o.Key != UserStatus.Offline && o.Key != UserStatus.Invisible)
                        .SelectMany(o => o)
                        .GroupBy(o => o.ActiveClients.FirstOrDefault())
                        .ToDictionary(o => o.Key, o => o.Count());

                    embed.AddField("Aktivní zařízení", "__ __", false)
                        .AddField("Desktop", devices.GetValueOrDefault(ClientType.Desktop), true)
                        .AddField("Mobilní", devices.GetValueOrDefault(ClientType.Mobile), true)
                        .AddField("Web", devices.GetValueOrDefault(ClientType.Web), true);

                    await ReplyAsync(embed: embed.Build());
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

            [Group("perms")]
            [Name("Správa oprávnění serveru")]
            [RequireBotPermission(GuildPermission.ManageChannels, ErrorMessage = "Nemohu spravovat oprávnění, protože nemám oprávnění na správu kanálů.")]
            [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu spravovat oprávnění, protože nemám oprávnění na správu rolí.")]
            [RequireUserPermission(GuildPermission.ManageRoles, ErrorMessage = "Tento příkaz může použít pouze uživatel, který může spravovat role.")]
            public class GuildPermissionsSubModule : Infrastructure.ModuleBase
            {
                private IConfiguration Configuration { get; }

                public GuildPermissionsSubModule(IConfiguration configuration)
                {
                    Configuration = configuration;
                }

                [Command("calc")]
                [Summary("Spočítá oprávnění na serveru")]
                public async Task CalcPermissionsAsync([Name("vse")] bool verbose = false)
                {
                    var guild = Context.Guild;

                    var channels = guild.Channels
                        .Where(o => o.PermissionOverwrites is ImmutableArray<Overwrite> overwriteArray && !overwriteArray.IsDefault)
                        .OrderBy(o => o.Position).Select(o => new
                        {
                            o.Position,
                            o.Name,
                            Type = o.GetType().Name.Replace("Rest", "").Replace("Socket", "").Replace("Channel", ""),
                            RolePermissions = o.PermissionOverwrites.AsEnumerable().Count(o => o.TargetId != guild.EveryoneRole.Id && o.TargetType == PermissionTarget.Role),
                            UserPermissions = o.PermissionOverwrites.AsEnumerable().Count(o => o.TargetId != guild.EveryoneRole.Id && o.TargetType == PermissionTarget.User)
                        }).Where(o => o.RolePermissions + o.UserPermissions > 0);

                    if (verbose)
                    {
                        var table = new DataTable();
                        table.Columns.AddRange(new[]
                        {
                            new DataColumn("Pozice", typeof(int)),
                            new DataColumn("Název", typeof(string)),
                            new DataColumn("Typ", typeof(string)),
                            new DataColumn("Počet práv rolí", typeof(int)),
                            new DataColumn("Počet uživatelských práv", typeof(int))
                        });

                        foreach (var channel in channels)
                        {
                            table.Rows.Add(channel.Position, channel.Name, channel.Type, channel.RolePermissions, channel.UserPermissions);
                        }

                        var formatedTable = ConsoleTableBuilder.From(table).Export().ToString();
                        var bytes = Encoding.UTF8.GetBytes(formatedTable);

                        using var ms = new MemoryStream(bytes);
                        await ReplyStreamAsync(ms, $"{guild.Name}_Report.txt", false);
                    }
                    else
                    {
                        var channelsData = channels.ToList();

                        var builder = new StringBuilder()
                            .Append("Počet kategorií: **").Append(channelsData.Count(o => o.Type == "Category")).AppendLine("**")
                            .Append("Počet textových kanálů: **").Append(channelsData.Count(o => o.Type == "Text")).AppendLine("**")
                            .Append("Počet hlasových kanálů: **").Append(channelsData.Count(o => o.Type == "Voice")).AppendLine("**").AppendLine()
                            .Append("Počet oprávnění (rolí): **").Append(channelsData.Sum(o => o.RolePermissions)).AppendLine("**")
                            .Append("Počet oprávnění (uživatelská): **").Append(channelsData.Sum(o => o.UserPermissions)).AppendLine("**");

                        await ReplyAsync(builder.ToString());
                    }
                }

                [Command("clear")]
                [Summary("Smaže všechna uživatelská oprávnění z kanálu.")]
                public async Task ClearPermissionsInChannelAsync([Name("kanal")] IGuildChannel channel, [Name("vynechani_uzivatele")] params IUser[] excludedUsers)
                {
                    await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));
                    await Context.Guild.DownloadUsersAsync();

                    var overwrites = channel.PermissionOverwrites.Where(o => o.TargetType == PermissionTarget.User && !excludedUsers.Any(x => x.Id == o.TargetId)).ToList();
                    var msg = await ReplyAsync($"Probíhá úklid oprávnění **0** / **{overwrites.Count}** (**0 %**)");

                    double removed = 0;
                    foreach (var overwrite in overwrites)
                    {
                        var user = Context.Guild.GetUser(overwrite.TargetId);
                        await channel.RemovePermissionOverwriteAsync(user);

                        removed++;
                        await msg.ModifyAsync(o => o.Content = $"Probíhá úklid oprávnění **{removed}** / **{overwrites.Count}** (**{Math.Round(removed / overwrites.Count * 100)} %**)");
                    }

                    await msg.ModifyAsync(o => o.Content = $"Úklid oprávnění dokončen. Smazáno **{removed}** uživatelských oprávnění.");
                    await Context.Message.RemoveAllReactionsAsync();
                    await Context.Message.AddReactionAsync(Emojis.Ok);
                }

                [Group("useless")]
                [Name("Zbytečná oprávnění")]
                [Summary("Detekce a smazání zbytečných oprávnění.")]
                [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nelze provést kontrolu zbytečných oprávnění, protože nemám oprávnění přidávat reakce.")]
                public class GuildUselessPermissionsSubModule : Infrastructure.ModuleBase
                {
                    private IMemoryCache Cache { get; }
                    private IConfiguration Configuration { get; }

                    public GuildUselessPermissionsSubModule(IMemoryCache cache, IConfiguration configuration)
                    {
                        Cache = cache;
                        Configuration = configuration;
                    }

                    [Command("check")]
                    [Summary("Zjistí zbytečná oprávnění a vygeneruje k nim report.")]
                    public async Task CheckUselessPermissionsAsync()
                    {
                        await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                        var uselessPermissions = await GetUselessPermissionsAsync();
                        var sessionId = Guid.NewGuid();

                        Cache.Set(sessionId, uselessPermissions);
                        var channelsCount = uselessPermissions.Select(o => o.Channel.Id).Distinct().Count();
                        var message = $"Kontrola zbytečných oprávnění dokončena.\nNalezeno zbytečných oprávnění: **{uselessPermissions.Count}**.\nPočet kanálů: **{channelsCount}**.\nTento výpočet je dostupný v cache pod klíčem `{sessionId}`";
                        await ReplyAsync(message);

                        await Context.Message.RemoveAllReactionsAsync();
                        await Context.Message.AddReactionAsync(Emojis.Ok);
                    }

                    [Command("report")]
                    [Summary("Vypíše data, která se nacházejí v reportu pod zadaným klíčem.")]
                    public async Task GetUselessPermissionsReportAsync([Name("klic_vypoctu")] Guid sessionId)
                    {
                        await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                        if (!Cache.TryGetValue<List<UselessPermission>>(sessionId, out var permissions))
                        {
                            await ReplyAsync("Nelze vygenerovat report, protože daná session neexistuje v cache.");
                            return;
                        }

                        var table = new DataTable();
                        table.Columns.AddRange(new[]
                        {
                            new DataColumn("Důvod"),
                            new DataColumn("Uživatelé")
                        });

                        foreach (var group in permissions.GroupBy(o => o.Type).Where(o => o.Any()))
                        {
                            var items = group.ToList();
                            for (int i = 0; i < items.Count; i++)
                            {
                                var item = items[i];
                                var value = $"{item.User.GetDisplayName()} (#{item.Channel.Name})";

                                table.Rows.Add(i == 0 ? group.Key.GetDescription() : " ", value);
                            }
                        }

                        var formatedTable = ConsoleTableBuilder.From(table)
                            .WithTitle($"Seznam zbytečných oprávnění ke dni {DateTime.Now:yyyy-MM-dd}")
                            .WithFormat(ConsoleTableBuilderFormat.MarkDown)
                            .Export()
                            .ToString();
                        var bytes = Encoding.UTF8.GetBytes(formatedTable);

                        using var ms = new MemoryStream(bytes);
                        await ReplyStreamAsync(ms, $"{Context.Guild.Name}_Report.md", false);
                        await Context.Message.RemoveAllReactionsAsync();
                        await Context.Message.AddReactionAsync(Emojis.Ok);
                    }

                    [Command("clear")]
                    [Summary("Smaže zbytečná oprávnění na základě předchozího výpočtu, případně nově provedeného výpočtu.")]
                    public async Task RemoveUselessPermissionsAsync([Name("klic_vypoctu")] Guid? sessionId = null)
                    {
                        await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                        if (sessionId == null || !Cache.TryGetValue<List<UselessPermission>>(sessionId, out var permissions))
                            permissions = await GetUselessPermissionsAsync();

                        var msg = await ReplyAsync($"Probíhá úklid oprávnění **0** / **{permissions.Count}** (**0 %**)");

                        double removed = 0;
                        foreach (var permission in permissions)
                        {
                            await permission.Channel.RemovePermissionOverwriteAsync(permission.User);

                            removed++;
                            await msg.ModifyAsync(o => o.Content = $"Probíhá úklid oprávnění **{removed}** / **{permissions.Count}** (**{Math.Round(removed / permissions.Count * 100)} %**)");
                        }

                        await msg.ModifyAsync(o => o.Content = $"Úklid oprávnění dokončen. Smazáno **{removed}** uživatelských oprávnění.");
                        await Context.Message.RemoveAllReactionsAsync();
                        await Context.Message.AddReactionAsync(Emojis.Ok);
                    }

                    private async Task<List<UselessPermission>> GetUselessPermissionsAsync()
                    {
                        await Context.Guild.DownloadUsersAsync();
                        var permissions = new List<UselessPermission>();

                        foreach (var user in Context.Guild.Users)
                        {
                            foreach (var channel in Context.Guild.Channels.Where(o => o.PermissionOverwrites is ImmutableArray<Overwrite> overwriteArray && !overwriteArray.IsDefault))
                            {
                                var overwrite = channel.GetPermissionOverwrite(user);
                                if (overwrite == null) continue; // Overwrite not exists. Skip.

                                if (user.GuildPermissions.Administrator)
                                {
                                    // User have Administrator permission. This user don't need some overwrites.
                                    permissions.Add(new UselessPermission(channel, user, UselessPermissionType.Administrator));
                                    continue;
                                }

                                if (overwrite.Value.AllowValue == 0 && overwrite.Value.DenyValue == 0)
                                {
                                    // Or user have neutral overwrite (overwrite without permissions).
                                    permissions.Add(new UselessPermission(channel, user, UselessPermissionType.Neutral));
                                    continue;
                                }

                                foreach (var role in user.Roles.OrderByDescending(o => o.Position))
                                {
                                    var roleOverwrite = channel.GetPermissionOverwrite(role);
                                    if (roleOverwrite == null) continue;

                                    if (roleOverwrite.Value.AllowValue == overwrite.Value.AllowValue && roleOverwrite.Value.DenyValue == overwrite.Value.DenyValue)
                                        permissions.Add(new UselessPermission(channel, user, UselessPermissionType.AvailableFromRole));
                                }
                            }
                        }

                        return permissions;
                    }
                }
            }

            [Group("react")]
            [Name("Správa reakcí na serveru")]
            [RequireBotPermission(GuildPermission.ManageMessages, ErrorMessage = "Nemohu spravovat reakce, protože nemám oprávnění pro správu zpráv.")]
            public class GuildReactSubModule : Infrastructure.ModuleBase
            {
                [Command("clear")]
                [Summary("Smaže reakci pro daný emote ze zprávy.")]
                [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "Tento příkaz může použít pouze uživatel, který může spravovat zprávy.")]
                public async Task RemoveReactionAsync([Name("zprava")] IMessage message, [Name("emote")] IEmote emote)
                {
                    await message.RemoveAllReactionsForEmoteAsync(emote);
                    await ReplyAsync($"Reakce pro emote {emote} byly smazány.");
                }
            }

            [Group("role")]
            [Name("Správa rolí")]
            [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu pracovat s rolemi, protože nemám dostatečná oprávnění.")]
            public class GuildRolesSubModule : Infrastructure.ModuleBase
            {
                [Group("info")]
                [Name("Informace o roli")]
                [RequireUserPremiumOrPermissions(GuildPermission.ViewGuildInsights, GuildPermission.ManageRoles, ErrorMessage =
                    "Tento příkaz může provést pouze uživatel, který vidí statistiky serveru ve vývojářském portálu, nebo může spravovat role, nebo má na serveru boost.")]
                public class GuildRoleInfoSubModule : Infrastructure.ModuleBase
                {
                    [Command("")]
                    [Alias("position")]
                    [Summary("Seznam rolí seřazených podle hierarchie.")]
                    public async Task GetRoleInfoListByPositionAsync()
                    {
                        await Context.Guild.DownloadUsersAsync();

                        var roles = GetFormatedRoleInfoQuery(o => o.Position).Take(EmbedBuilder.MaxFieldCount).ToList();
                        var color = Context.Guild.GetHighestRole(true)?.Color ?? Color.Default;
                        var summary = CreateRoleInfoSummary();

                        var embed = CreateRoleInfoEmbed(roles, color, summary);
                        await ReplyAsync(embed: embed.Build());
                    }

                    [Command("members")]
                    [Summary("Seznam rolí seřazeno podle počtu uživatelů s přiřazenou rolí.")]
                    public async Task GetRoleInfoListByMemberCountAsync()
                    {
                        await Context.Guild.DownloadUsersAsync();

                        var roles = GetFormatedRoleInfoQuery(o => o.Members.Count()).Take(EmbedBuilder.MaxFieldCount).ToList();
                        var color = Context.Guild.Roles.OrderByDescending(o => o.Members.Count()).FirstOrDefault(o => o.Color != Color.Default)?.Color ?? Color.Default;
                        var summary = CreateRoleInfoSummary();

                        var embed = CreateRoleInfoEmbed(roles, color, summary);
                        await ReplyAsync(embed: embed.Build());
                    }

                    [Command("")]
                    [Summary("Detailní informace o roli.")]
                    public async Task GetRoleInfoAsync(SocketRole role)
                    {
                        await Context.Guild.DownloadUsersAsync();

                        var fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder().WithIsInline(true).WithName("Vytvořeno").WithValue(role.CreatedAt.LocalDateTime.ToCzechFormat()),
                            new EmbedFieldBuilder().WithIsInline(true).WithName("Everyone").WithValue(FormatHelper.FormatBooleanToCzech(role.IsEveryone)),
                            new EmbedFieldBuilder().WithIsInline(true).WithName("Separovaná").WithValue(FormatHelper.FormatBooleanToCzech(role.IsHoisted)),
                            new EmbedFieldBuilder().WithIsInline(true).WithName("Nespravovatelná").WithValue(FormatHelper.FormatBooleanToCzech(role.IsManaged)),
                            new EmbedFieldBuilder().WithIsInline(true).WithName("Tagovatelná").WithValue(FormatHelper.FormatBooleanToCzech(role.IsMentionable)),
                        };

                        if (role.Tags?.BotId == null)
                            fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Počet členů").WithValue(FormatHelper.FormatMembersToCzech(role.Members.Count())));

                        if (role.Tags != null)
                        {
                            if (role.Tags.IsPremiumSubscriberRole)
                                fields.Add(new EmbedFieldBuilder().WithName("Booster role").WithValue("Ano").WithIsInline(true));

                            if (role.Tags.BotId != null)
                            {
                                var botUser = Context.Guild.GetUser(role.Tags.BotId.Value);

                                if (botUser != null)
                                    fields.Add(new EmbedFieldBuilder().WithName("Náleží botovi").WithValue($"`{botUser.GetFullName()}`").WithIsInline(false));
                            }
                        }

                        var formatedPerms = role.Permissions.Administrator ? new List<string>() { "Administrator" } : role.Permissions.ToList().ConvertAll(o => o.ToString());
                        fields.Add(new EmbedFieldBuilder().WithName("Oprávnění").WithValue(string.Join(", ", formatedPerms)).WithIsInline(false));

                        var embed = CreateRoleInfoEmbed(fields, role.Color, null)
                            .WithTitle(role.Name);

                        await ReplyAsync(embed: embed.Build());
                    }

                    private string CreateRoleInfoSummary()
                    {
                        var totalMembersWithRole = Context.Guild.Users.Count(o => o.Roles.Any(o => !o.IsEveryone)); // Count of users with some role.
                        var membersWithoutRole = Context.Guild.Users.Count(o => o.Roles.All(o => o.IsEveryone)); // Count of users without some role.

                        return $"Počet rolí: {Context.Guild.Roles.Count}\nPočet uživatelů s rolí: {totalMembersWithRole}\nPočet uživatelů bez role: {membersWithoutRole}";
                    }

                    private IEnumerable<EmbedFieldBuilder> GetFormatedRoleInfoQuery<TKey>(Func<SocketRole, TKey> orderBySelector)
                    {
                        return Context.Guild.Roles.Where(o => !o.IsEveryone).OrderByDescending(orderBySelector).Select(o =>
                        {
                            var info = string.Join(", ", new[]
                            {
                                FormatHelper.FormatMembersToCzech(o.Members.Count()),
                                $"vytvořeno {o.CreatedAt.LocalDateTime.ToCzechFormat()}",
                                o.IsMentionable ? "tagovatelná" : "",
                                o.IsManaged ? "spravuje Discord" : "",
                                o.Tags?.IsPremiumSubscriberRole == true ? "booster" : ""
                            }.Where(o => !string.IsNullOrEmpty(o)));

                            return new EmbedFieldBuilder().WithName(o.Name).WithValue(info);
                        });
                    }

                    private EmbedBuilder CreateRoleInfoEmbed(List<EmbedFieldBuilder> fields, Color color, string summary)
                    {
                        return new EmbedBuilder()
                            .WithFooter(Context.User)
                            .WithColor(color)
                            .WithCurrentTimestamp()
                            .WithDescription(summary)
                            .WithFields(fields)
                            .WithTitle("Seznam rolí");
                    }
                }
            }

            [Group("invite")]
            [Summary("Správa pozvánek")]
            [Name("Pozvánky")]
            [RequireBotPermission(GuildPermission.CreateInstantInvite, ErrorMessage = "Nemohu pracovat s pozvánkami, protože nemám oprávnění pro vytvoření pozvánek.")]
            [RequireBotPermission(GuildPermission.ManageGuild, ErrorMessage = "Nemohu pracovat s pozvánkami, protože nemám oprávnění pracovat se serverem.")]
            [RequireUserPermission(GuildPermission.ManageGuild, ErrorMessage = "Tento příkaz může použít pouze uživatel, který může spravovat server.")]
            public class GuildInvitesSubModule : Infrastructure.ModuleBase
            {
                private InviteService InviteService { get; }

                public GuildInvitesSubModule(InviteService inviteService)
                {
                    InviteService = inviteService;
                }

                [Command("assign")]
                [Summary("Přiřazení pozvánky k uživateli.")]
                public async Task AssignCodeToUserAsync([Name("id/tag/jmeno_uzivatele")] SocketUser user, [Name("kod_pozvanky")] string code)
                {
                    if (string.Equals(code, "vanity", StringComparison.InvariantCultureIgnoreCase))
                        code = Context.Guild.VanityURLCode;

                    try
                    {
                        await InviteService.AssignInviteToUserAsync(user, Context.Guild, code);
                        await ReplyAsync("Pozvánka byla úspěšně přiřazena.");
                    }
                    catch (NotFoundException ex)
                    {
                        await ReplyAsync(ex.Message);
                    }
                }

                [Command("refresh")]
                [Alias("update")]
                [Summary("Obnovení pozvánek v paměti.")]
                public async Task RefreshCacheAsync()
                {
                    var updatedCount = await InviteService.RefreshMetadataOfGuildAsync(Context.Guild);
                    await ReplyAsync($"Pozvánky pro server **{Context.Guild.Name}** byly staženy. Celkový počet pozvánek je **{updatedCount}**.");
                }
            }
        }
    }
}
