using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

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
        [TextCommandDeprecated(AlternativeCommand = "/channel send")]
        public Task SendAnonymousToChannelAsync(IMessageChannel channel, string content = null) => Task.CompletedTask;

        [Command("info")]
        [TextCommandDeprecated(AlternativeCommand = "/guild info")]
        public Task InfoAsync() => Task.CompletedTask;

        [Group("perms")]
        [Name("Správa oprávnění serveru")]
        [RequireBotPermission(GuildPermission.ManageChannels, ErrorMessage = "Nemohu spravovat oprávnění, protože nemám oprávnění na správu kanálů.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu spravovat oprávnění, protože nemám oprávnění na správu rolí.")]
        [RequireUserPerms(GuildPermission.ManageRoles)]
        public class GuildPermissionsSubModule : ModuleBase
        {
            [Command("clear")]
            public Task ClearPermissionsInChannelAsync(IGuildChannel channel, params IUser[] excludedUsers) => Task.CompletedTask;

            [Group("useless")]
            [Name("Zbytečná oprávnění")]
            [Summary("Detekce a smazání zbytečných oprávnění.")]
            [RequireBotPermission(GuildPermission.AddReactions, ErrorMessage = "Nelze provést kontrolu zbytečných oprávnění, protože nemám oprávnění přidávat reakce.")]
            public class GuildUselessPermissionsSubModule : ModuleBase
            {
                [Command("check")]
                [TextCommandDeprecated(AlternativeCommand = "/permissions useless check")]
                public Task CheckUselessPermissionsAsync() => Task.CompletedTask;

                [Command("clear")]
                [TextCommandDeprecated(AlternativeCommand = "/permissions useless clear")]
                public Task RemoveUselessPermissionsAsync(Guid? sessionId = null) => Task.CompletedTask;
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
