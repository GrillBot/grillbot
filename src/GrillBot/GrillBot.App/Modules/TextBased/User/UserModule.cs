using Discord.Commands;
using GrillBot.App.Modules.Implementations.User;
using GrillBot.App.Services.User;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Extensions;
using RequireUserPerms = GrillBot.App.Infrastructure.Preconditions.TextBased.RequireUserPermsAttribute;

namespace GrillBot.App.Modules.TextBased.User;

[Group("user")]
[Name("Správa uživatelů")]
[RequireContext(ContextType.Guild, ErrorMessage = "Tento příkaz lze použít pouze na serveru.")]
public class UserModule : Infrastructure.ModuleBase
{
    private UserService UserService { get; }

    public const int UserAccessMaxCountPerPage = 15;

    public UserModule(UserService userService)
    {
        UserService = userService;
    }

    [Command("info")]
    [Summary("Získání informací o uživateli.")]
    [RequireUserPerms(GuildPermission.ViewAuditLog)]
    public async Task GetUserInfoAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
    {
        if (user == null) user = Context.User;
        if (user is not SocketGuildUser guildUser) return;

        var embed = await UserService.CreateInfoEmbed(Context.User, Context.Guild, guildUser);
        await ReplyAsync(embed: embed);
    }

    [Command("access")]
    [Summary("Získání seznamu přístupů uživatelů.")]
    [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění v kanálech.")]
    [RequireUserPerms(GuildPermission.ManageRoles)]
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
            var visibleChannels = GetUserVisibleChannels(Context.Guild, guildUser).Take(UserAccessMaxCountPerPage).ToList();
            var embed = new EmbedBuilder().WithUserAccessList(visibleChannels, guildUser, Context.User, Context.Guild, 0);

            var message = await ReplyAsync(embed: embed.Build());
            if (visibleChannels.Count >= UserAccessMaxCountPerPage)
                await message.AddReactionsAsync(new[] { Emojis.MoveToPrev, Emojis.MoveToNext });
        }
    }

    [Command("access")]
    [Summary("Získání seznamu přístupů uživatelů na základě role.")]
    [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění spravovat oprávnění v kanálech.")]
    [RequireUserPerms(GuildPermission.ManageRoles)]
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
