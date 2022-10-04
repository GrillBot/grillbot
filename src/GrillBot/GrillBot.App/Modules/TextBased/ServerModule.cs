using Discord.Commands;
using System.Net.Http;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using ConsoleTableExt;
using Microsoft.Extensions.Caching.Memory;
using GrillBot.Data.Models.Guilds;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.Permissions;
using GrillBot.App.Services.Unverify;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Name("Správa serveru")]
public class ServerModule : ModuleBase
{
    [Command("clean")]
    [TextCommandDeprecated(AlternativeCommand = "/channel clean")]
    public Task CleanAsync(int take, ITextChannel channel = null) => Task.CompletedTask;

    [Group("pin")]
    [TextCommandDeprecated(AlternativeCommand = "/pin purge")]
    public class PinManagementSubmodule : ModuleBase
    {
        [Command("purge")]
        public Task PurgePinsAsync(ITextChannel channel = null, params ulong[] messageIds) => Task.CompletedTask;

        [Command("purge")]
        public Task PurgePinsAsync(int count, ITextChannel channel = null) => Task.CompletedTask;
    }

    [Group("guild")]
    [Name("Servery")]
    public class GuildManagementSubmodule : ModuleBase
    {
        [Command("send")]
        [Summary("Pošle zprávu (vč. příloh) do kanálu.")]
        [RequireBotPermission(GuildPermission.ManageMessages, ErrorMessage = "Nemohu tenhle příkaz provést, protože nemám oprávnění mazat zprávy.")]
        [RequireUserPerms(GuildPermission.ManageMessages)]
        public async Task SendAnonymousToChannelAsync([Name("kanal")] IMessageChannel channel, [Remainder] [Name("volitelna_zprava")] string content = null)
        {
            if (string.IsNullOrEmpty(content) && Context.Message.ReferencedMessage != null)
                content = Context.Message.ReferencedMessage.Content;

            var attachments = Context.Message.Attachments.Select(o => o as IAttachment).ToList();
            if (attachments.Count > 0 && Context.Message.ReferencedMessage != null) attachments = Context.Message.ReferencedMessage.Attachments.ToList();

            if (string.IsNullOrEmpty(content) && attachments.Count == 0)
            {
                await ReplyAsync("Nemůžu nic poslat, protože jsi mi nic nedal.");
                return;
            }

            if (attachments.Count > 0)
            {
                using var httpClient = new HttpClient();

                var firstDone = string.IsNullOrEmpty(content);
                foreach (var attachment in attachments)
                {
                    var response = await httpClient.GetAsync(attachment.Url);

                    if (!response.IsSuccessStatusCode)
                        continue;

                    var stream = await response.Content.ReadAsStreamAsync();
                    var spoiler = attachment.IsSpoiler();

                    if (firstDone)
                    {
                        await channel.SendFileAsync(stream, attachment.Filename, null, false, null, null, spoiler, AllowedMentions);
                    }
                    else
                    {
                        await channel.SendFileAsync(stream, attachment.Filename, content, isSpoiler: spoiler, allowedMentions: AllowedMentions);

                        firstDone = true;
                    }
                }
            }
            else
            {
                await channel.SendMessageAsync(content);
            }

            await Context.Message.DeleteAsync();
        }

        [Command("info")]
        [TextCommandDeprecated(AlternativeCommand = "/guild info")]
        [ExcludeFromCodeCoverage]
        public Task InfoAsync() => Task.CompletedTask;

        [Group("perms")]
        [Name("Správa oprávnění serveru")]
        [RequireBotPermission(GuildPermission.ManageChannels, ErrorMessage = "Nemohu spravovat oprávnění, protože nemám oprávnění na správu kanálů.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu spravovat oprávnění, protože nemám oprávnění na správu rolí.")]
        [RequireUserPerms(GuildPermission.ManageRoles)]
        public class GuildPermissionsSubModule : ModuleBase
        {
            private IConfiguration Configuration { get; }

            public GuildPermissionsSubModule(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            [Command("clear")]
            [Summary("Smaže všechna uživatelská oprávnění z kanálu.")]
            public async Task ClearPermissionsInChannelAsync([Name("kanal")] IGuildChannel channel, [Name("vynechani_uzivatele")] params IUser[] excludedUsers)
            {
                await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));
                await Context.Guild.DownloadUsersAsync();

                var overwrites = channel.PermissionOverwrites.Where(o => o.TargetType == PermissionTarget.User && excludedUsers.All(x => x.Id != o.TargetId)).ToList();
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

            [Command("remove user")]
            [Summary("Smaže oprávnění uživatele v kanálech.")]
            public async Task RemoveUserFromChannelsAsync([Name("id/tag/jmeno_uzivatele")] IGuildUser user, [Name("kanaly")] params IGuildChannel[] guildChannels)
            {
                var channels = guildChannels
                    .Select(o => o is SocketCategoryChannel category ? category.Channels.OfType<IGuildChannel>() : new[] { o })
                    .SelectMany(o => o)
                    .Distinct()
                    .ToArray();

                if (channels.Length == 0) return;
                await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                var formatMessage = new Func<double, string>(removed => $"Probíhá úklid oprávnění **{removed}** / **{channels.Length}** (**{Math.Round(removed / channels.Length * 100)} %**)");
                var msg = await ReplyAsync(formatMessage(0));

                double removed = 0;
                foreach (var channel in channels)
                {
                    var userPermission = channel.GetPermissionOverwrite(user);
                    if (userPermission != null)
                        await channel.RemovePermissionOverwriteAsync(user);

                    removed++;
                    await msg.ModifyAsync(o => o.Content = formatMessage(removed));
                }

                await msg.ModifyAsync(o => o.Content = $"Úklid oprávnění dokončen. Smazáno **{removed}** uživatelských oprávnění.");
                await Context.Message.RemoveAllReactionsAsync();
                await Context.Message.AddReactionAsync(Emojis.Ok);
            }

            [Group("useless")]
            [Name("Zbytečná oprávnění")]
            [Summary("Detekce a smazání zbytečných oprávnění.")]
            [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nelze provést kontrolu zbytečných oprávnění, protože nemám oprávnění přidávat reakce.")]
            public class GuildUselessPermissionsSubModule : ModuleBase
            {
                private IMemoryCache Cache { get; }
                private IConfiguration Configuration { get; }
                private PermissionsCleaner PermissionsCleaner { get; }
                private UnverifyService UnverifyService { get; }

                public GuildUselessPermissionsSubModule(IMemoryCache cache, IConfiguration configuration, PermissionsCleaner permissionsCleaner, UnverifyService unverifyService)
                {
                    Cache = cache;
                    Configuration = configuration;
                    PermissionsCleaner = permissionsCleaner;
                    UnverifyService = unverifyService;
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
                    var message =
                        $"Kontrola zbytečných oprávnění dokončena.\nNalezeno zbytečných oprávnění: **{uselessPermissions.Count}**.\nPočet kanálů: **{channelsCount}**.\nTento výpočet je dostupný v cache pod klíčem `{sessionId}`";
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
                        for (var i = 0; i < items.Count; i++)
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
                        await PermissionsCleaner.RemoveUselessPermissionAsync(permission);

                        removed++;
                        if ((removed % 2) == 0)
                            await msg.ModifyAsync(o => o.Content = $"Probíhá úklid oprávnění **{removed}** / **{permissions.Count}** (**{Math.Round(removed / permissions.Count * 100)} %**)");
                    }

                    await msg.ModifyAsync(o => o.Content = $"Úklid oprávnění dokončen. Smazáno **{removed}** uživatelských oprávnění.");
                    await Context.Message.RemoveAllReactionsAsync();
                    await Context.Message.AddReactionAsync(Emojis.Ok);
                }

                [Command("clearForChannel")]
                [Summary("Smaže zbytečná oprávnění pro daný kanál.")]
                public async Task RemoveUselessPermissionsFromChannelAsync([Name("kanal")] IGuildChannel channel)
                {
                    await Context.Message.AddReactionAsync(Emote.Parse(Configuration["Discord:Emotes:Loading"]));

                    try
                    {
                        var unverifyIds = await UnverifyService.GetUserIdsWithUnverifyAsync(channel.Guild);
                        var permissions = await PermissionsCleaner.GetUselessPermissionsForChannelAsync(channel, channel.Guild);
                        permissions = permissions.FindAll(o => !unverifyIds.Contains(o.User.Id));

                        if (permissions.Count == 0)
                        {
                            await ReplyAsync($"Nebylo nalezeno žádné zbytečné oprávnění pro kanál {channel.Name}");
                            return;
                        }

                        var msg = await ReplyAsync(
                            $"Probíhá úklid oprávnění **0** / **{permissions.Count}** (**0 %**)");

                        double removed = 0;
                        foreach (var permission in permissions)
                        {
                            await PermissionsCleaner.RemoveUselessPermissionAsync(permission);

                            removed++;
                            if ((removed % 2) == 0)
                                await msg.ModifyAsync(o => o.Content = $"Probíhá úklid oprávnění **{removed}** / **{permissions.Count}** (**{Math.Round(removed / permissions.Count * 100)} %**)");
                        }

                        await msg.ModifyAsync(o =>
                            o.Content = $"Úklid oprávnění dokončen. Smazáno **{removed}** uživatelských oprávnění.");
                    }
                    finally
                    {
                        await Context.Message.RemoveAllReactionsAsync();
                        await Context.Message.AddReactionAsync(Emojis.Ok);
                    }
                }

                private async Task<List<UselessPermission>> GetUselessPermissionsAsync()
                {
                    var unverifyUsers = await UnverifyService.GetUserIdsWithUnverifyAsync(Context.Guild);

                    await Context.Guild.DownloadUsersAsync();
                    var permissions = new List<UselessPermission>();

                    foreach (var user in Context.Guild.Users.Where(o => !unverifyUsers.Contains(o.Id)))
                    {
                        try
                        {
                            var uselessPermissions = await PermissionsCleaner.GetUselessPermissionsForUser(user, Context.Guild);
                            permissions.AddRange(uselessPermissions);
                        }
                        catch (InvalidOperationException)
                        {
                            // Can ignore
                        }
                    }

                    return permissions;
                }
            }
        }

        [Group("react")]
        [Name("Správa reakcí na serveru")]
        [RequireBotPermission(GuildPermission.ManageMessages, ErrorMessage = "Nemohu spravovat reakce, protože nemám oprávnění pro správu zpráv.")]
        public class GuildReactSubModule : ModuleBase
        {
            [Command("clear")]
            [Summary("Smaže reakci pro daný emote ze zprávy.")]
            [RequireUserPerms(GuildPermission.ManageMessages)]
            public async Task RemoveReactionAsync([Name("zprava")] IMessage message, [Name("emote")] IEmote emote)
            {
                await message.RemoveAllReactionsForEmoteAsync(emote);
                await ReplyAsync($"Reakce pro emote {emote} byly smazány.");
            }
        }

        [Group("role")]
        [Name("Správa rolí")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu pracovat s rolemi, protože nemám dostatečná oprávnění.")]
        public class GuildRolesSubModule : ModuleBase
        {
            [Group("info")]
            [Name("Informace o roli")]
            [RequireUserPerms(GuildPermission.ManageRoles)]
            public class GuildRoleInfoSubModule : ModuleBase
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

                    var fields = new List<EmbedFieldBuilder>
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

                    var formatedPerms = role.Permissions.Administrator ? new List<string> { "Administrator" } : role.Permissions.ToList().ConvertAll(o => o.ToString());
                    if (formatedPerms.Count > 0)
                        fields.Add(new EmbedFieldBuilder().WithName("Oprávnění").WithValue(string.Join(", ", formatedPerms)).WithIsInline(false));

                    var embed = CreateRoleInfoEmbed(fields, role.Color, null)
                        .WithTitle(role.Name);

                    await ReplyAsync(embed: embed.Build());
                }

                private string CreateRoleInfoSummary()
                {
                    var totalMembersWithRole = Context.Guild.Users.Count(o => o.Roles.Any(x => !x.IsEveryone)); // Count of users with some role.
                    var membersWithoutRole = Context.Guild.Users.Count(o => o.Roles.All(x => x.IsEveryone)); // Count of users without some role.

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
                        }.Where(x => !string.IsNullOrEmpty(x)));

                        return new EmbedFieldBuilder().WithName(o.Name).WithValue(info);
                    });
                }

                private EmbedBuilder CreateRoleInfoEmbed(IEnumerable<EmbedFieldBuilder> fields, Color color, string summary)
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
    }
}
