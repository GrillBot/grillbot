using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RequireUserPermsAttribute = GrillBot.App.Infrastructure.Preconditions.RequireUserPermissionAttribute;

namespace GrillBot.App.Modules.User
{
    [Group("user")]
    [Name("Správa uživatelů")]
    [RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
    public class UserModule : Infrastructure.ModuleBase
    {
        private IConfiguration Configuration { get; }
        private GrillBotContextFactory DbFactory { get; }

        public UserModule(IConfiguration configuration, GrillBotContextFactory dbFactory)
        {
            Configuration = configuration;
            DbFactory = dbFactory;
        }

        [Command("info")]
        [Summary("Získání informací o uživateli.")]
        [RequireUserPerms(new[] { GuildPermission.ViewAuditLog }, true)]
        public async Task GetUserInfoAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
        {
            if (user == null) user = Context.User;
            if (user is not SocketGuildUser guildUser) return;

            var embed = await GetUserInfoEmbedAsync(Context, DbFactory, Configuration, guildUser);
            await ReplyAsync(embed: embed);
        }

        [Command("access")]
        [Summary("Získání seznamu přístupů uživatelů.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění v kanálech.")]
        [RequireUserPerms(new[] { GuildPermission.ManageRoles }, false)]
        public async Task GetUsersAccessListAsync([Name("id/tagy/jmena_uzivatelu")] params IUser[] users)
        {
            if (users == null || users.Length == 0) users = new[] { Context.User };
            users = users.OfType<SocketGuildUser>().ToArray();
            if (users.Length == 0) return;

            await Context.Guild.DownloadUsersAsync();
            if (users.Length > 1)
            {
                var channels = users.Select(o => o as SocketGuildUser).ToDictionary(
                    o => o,
                    o => GetUserVisibleChannels(Context.Guild, o).SelectMany(o => o.Item2).SplitInParts(20).ToList()
                );

                var fields = channels.SelectMany(o =>
                    o.Value.ConvertAll(x => new EmbedFieldBuilder().WithName(o.Key.GetFullName()).WithValue(string.Join(" ", x)))
                ).Take(EmbedBuilder.MaxFieldCount).ToList();

                var embed = new EmbedBuilder()
                    .WithFooter(Context.User)
                    .WithAuthor("Seznam oprávnění pro uživatele")
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .WithFields(fields)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            else
            {
                var guildUser = users[0] as SocketGuildUser;
                var visibleChannels = GetUserVisibleChannels(Context.Guild, guildUser).Take(EmbedBuilder.MaxFieldCount).ToList();
                var embed = new EmbedBuilder().WithUserAccessList(visibleChannels, guildUser, Context.User, Context.Guild, 0);

                var message = await ReplyAsync(embed: embed.Build());
                if (visibleChannels.Count >= EmbedBuilder.MaxFieldCount)
                    await message.AddReactionsAsync(new[] { Emojis.MoveToPrev, Emojis.MoveToNext });
            }
        }

        [Command("access")]
        [Summary("Získání seznamu přístupů uživatelů na základě role.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění v kanálech.")]
        [RequireUserPerms(new[] { GuildPermission.ManageRoles }, false)]
        public async Task GetUsersAccessListAsync([Name("id/tag/jmeno_role")] SocketRole role)
        {
            if (role.IsEveryone)
            {
                await ReplyAsync("Nezle provést detekci přístupu pro everyone roli.");
                return;
            }

            var members = role.Members.Where(o => o.IsUser()).ToArray();
            await GetUsersAccessListAsync(members);
        }

        public static async Task<Embed> GetUserInfoEmbedAsync(SocketCommandContext context, GrillBotContextFactory dbFactory, IConfiguration configuration,
            SocketGuildUser user)
        {
            await context.Guild.DownloadUsersAsync();
            using var dbContext = dbFactory.Create();

            var userDetailUrl = await CreateWebAdminLink(dbContext, context, configuration, user);
            var highestRoleWithColor = user.GetHighestRole(true);
            var state = GetStateEmote(user, configuration, out var userStatus);
            var joinPosition = CalculateJoinPosition(user, user.Guild);
            var roles = user.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Mention);
            var clients = user.ActiveClients.Select(o => o.ToString());

            var embed = new EmbedBuilder()
                .WithAuthor(user.GetFullName(), user.GetAvatarUri(), userDetailUrl)
                .WithCurrentTimestamp()
                .WithFooter(context.User)
                .WithTitle("Informace o uživateli")
                .WithColor(highestRoleWithColor?.Color ?? Color.Default)
                .AddField("Stav", $"{state} {userStatus}", false)
                .AddField("Role", roles.Any() ? string.Join(" ", roles) : "*Uživatel nemá žádné role.*", false)
                .AddField("Založen", user.CreatedAt.LocalDateTime.ToCzechFormat(), true)
                .AddField("Připojen (pořadí)", $"{user.JoinedAt.Value.LocalDateTime.ToCzechFormat()} ({joinPosition}.)", true);

            if (user.PremiumSince != null)
                embed.AddField("Boost od", user.PremiumSince.Value.LocalDateTime.ToCzechFormat(), true);

            if (clients.Any())
                embed.AddField("Aktivní zařízení", string.Join(", ", clients), false);

            var userStateQueryBase = dbContext.GuildUsers.AsQueryable()
                .AsNoTracking()
                .Where(o => o.GuildId == context.Guild.Id.ToString() && o.UserId == user.Id.ToString());

            var baseData = await userStateQueryBase.Include(o => o.UsedInvite)
                .Select(o => new { o.Points, o.GivenReactions, o.ObtainedReactions, o.UsedInvite }).FirstOrDefaultAsync();
            if (baseData != null)
            {
                embed.AddField("Body", baseData.Points, true)
                    .AddField("Udělené reakce", baseData?.GivenReactions, true)
                    .AddField("Obdržené reakce", baseData?.ObtainedReactions, true);
            }

            var messagesCount = await dbContext.UserChannels.AsQueryable().Where(o => o.Count > 0 && o.GuildId == context.Guild.Id.ToString() && o.UserId == user.Id.ToString()).SumAsync(o => o.Count);
            embed.AddField("Počet zpráv", messagesCount, true);

            var unverifyData = await dbContext.UnverifyLogs.AsQueryable()
                .Where(o => o.ToUserId == user.Id.ToString() && o.GuildId == context.Guild.Id.ToString())
                .GroupBy(o => o.Operation).Select(o => new
                {
                    Unverify = o.Count(_ => o.Key == UnverifyOperation.Unverify),
                    Selfunverify = o.Count(_ => o.Key == UnverifyOperation.Selfunverify)
                }).FirstOrDefaultAsync();

            if (unverifyData != null)
            {
                embed.AddField("Počet unverify", unverifyData.Unverify, true)
                    .AddField("Počet selfunverify", unverifyData.Selfunverify, true);
            }

            if (baseData?.UsedInvite != null)
            {
                var invite = baseData.UsedInvite;
                bool isVanity = invite.Code == context.Guild.VanityURLCode;
                var creator = isVanity ? null : await context.Client.FindUserAsync(Convert.ToUInt64(invite.CreatorId));

                embed.AddField(
                    "Použitá pozvánka",
                    $"**{invite.Code}**\n{(isVanity ? "Vanity invite" : $"Založil: **{creator?.GetFullName()}** (**{invite.CreatedAt?.ToCzechFormat()}**)")}",
                    false
                );
            }

            var channelActivity = await userStateQueryBase.Select(o => new
            {
                MostActive = o.User.Channels.Where(x => x.GuildId == o.GuildId).OrderByDescending(o => o.Count).Select(o => new { o.Id, o.Count }).FirstOrDefault(),
                LastMessage = o.User.Channels.Where(x => x.GuildId == o.GuildId).OrderByDescending(o => o.LastMessageAt).Select(o => new { o.Id, o.LastMessageAt }).FirstOrDefault()
            }).FirstOrDefaultAsync();

            if (channelActivity != null)
            {
                if (channelActivity.MostActive != null)
                    embed.AddField("Nejaktivnější kanál", $"<#{channelActivity.MostActive.Id}> ({channelActivity.MostActive.Count})", false);

                if (channelActivity.LastMessage != null)
                    embed.AddField("Poslední zpráva", $"<#{channelActivity.LastMessage.Id}> ({channelActivity.LastMessage.LastMessageAt.ToCzechFormat()})", false);
            }

            return embed.Build();
        }

        private static async Task<string> CreateWebAdminLink(GrillBotContext dbContext, SocketCommandContext context, IConfiguration configuration, IUser user)
        {
            var userData = await dbContext.Users.AsQueryable()
                .Select(o => new Database.Entity.User() { Id = o.Id, Flags = o.Flags })
                .FirstOrDefaultAsync(o => o.Id == context.User.Id.ToString());

            if (userData?.HaveFlags(UserFlags.WebAdmin) != true)
                return null;

            var value = configuration.GetValue<string>("WebAdmin:UserDetailLink");
            return string.Format(value, user.Id);
        }

        private static Emote GetStateEmote(IUser user, IConfiguration configuration, out string userStatus)
        {
            var status = user.Status;
            if (status == UserStatus.AFK) status = UserStatus.Idle;
            else if (status == UserStatus.Invisible) status = UserStatus.Offline;

            userStatus = status switch
            {
                UserStatus.DoNotDisturb => "Nerušit",
                UserStatus.Idle => "Nepřítomen",
                _ => status.ToString(),
            };

            var emoteData = configuration.GetValue<string>($"Discord:Emotes:{status}");
            return Emote.Parse(emoteData);
        }

        private static int CalculateJoinPosition(SocketGuildUser user, SocketGuild guild)
        {
            return guild.Users.Count(o => o.JoinedAt <= user.JoinedAt);
        }

        public static IEnumerable<Tuple<string, List<string>>> GetUserVisibleChannels(SocketGuild guild, SocketGuildUser user)
        {
            var channels = guild.GetAvailableChannelsFor(user).GroupBy(o =>
            {
                if (o is SocketTextChannel text && !string.IsNullOrEmpty(text.Category?.Name)) return text.Category.Name;
                else if (o is SocketVoiceChannel voice && !string.IsNullOrEmpty(voice.Category?.Name)) return voice.Category.Name;
                return "Bez kategorie";
            }).Select(o => new { Category = o.Key, ChannelGroups = o.SplitInParts(20).Select(x => x.OrderBy(o => o.Position).Select(t => t.GetMention())) })
            .Select(o => new Tuple<string, IEnumerable<IEnumerable<string>>>(o.Category, o.ChannelGroups));

            return RegroupChannels(channels);
        }

        private static IEnumerable<Tuple<string, List<string>>> RegroupChannels(IEnumerable<Tuple<string, IEnumerable<IEnumerable<string>>>> categories)
        {
            foreach (var category in categories)
            {
                foreach (var group in category.Item2)
                {
                    yield return new Tuple<string, List<string>>(category.Item1, group.ToList());
                }
            }
        }
    }
}
