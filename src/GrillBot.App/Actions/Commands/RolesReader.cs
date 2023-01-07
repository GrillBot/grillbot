using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands;

public class RolesReader : CommandAction
{
    private FormatHelper FormatHelper { get; }
    private ITextsManager Texts { get; }

    private IReadOnlyCollection<IGuildUser> Users { get; set; }

    public RolesReader(FormatHelper formatHelper, ITextsManager texts)
    {
        FormatHelper = formatHelper;
        Texts = texts;
    }

    public async Task<Embed> ProcessListAsync(string sortBy)
    {
        Users = await Context.Guild.GetUsersAsync();

        var fields = GetRoleListFields(sortBy);
        var color = Context.Guild.GetHighestRole(true)?.Color ?? Color.Default;
        var guildSummary = GetGuildRolesSummary();
        var title = Texts["Roles/ListTitle", Locale];

        return CreateEmbed(fields, color, title, guildSummary);
    }

    private IEnumerable<EmbedFieldBuilder> GetRoleListFields(string sortBy)
    {
        var query = Context.Guild.Roles
            .Where(o => o.Id != Context.Guild.EveryoneRole.Id)
            .Select(GetRoleSummary);

        query = sortBy switch
        {
            "members" => query.OrderByDescending(o => o.membersCount),
            _ => query.OrderByDescending(o => o.position)
        };

        return query.Take(EmbedBuilder.MaxFieldCount).Select(o => o.field);
    }

    private (EmbedFieldBuilder field, int membersCount, int position) GetRoleSummary(IRole role)
    {
        var membersCount = Users.Count(o => o.RoleIds.Contains(role.Id));

        var memberCount = FormatHelper.FormatNumber("Roles/MemberCounts", Locale, membersCount);
        var mentionable = role.IsMentionable ? Texts["Roles/Mentionable", Locale] : "";
        var managed = role.IsManaged ? Texts["Roles/Managed", Locale] : "";
        var premium = role.Tags?.IsPremiumSubscriberRole == true ? Texts["Roles/PremiumSubscriberRole", Locale] : "";
        var createdAt = role.CreatedAt.LocalDateTime.ToCzechFormat();

        var summaryLine = Texts["Roles/RoleSummaryLine", Locale].FormatWith(memberCount, createdAt, mentionable, managed, premium);
        summaryLine = summaryLine.TrimEnd(',', ' ');

        return (new EmbedFieldBuilder().WithName(role.Name).WithValue(summaryLine), membersCount, role.Position);
    }

    private string GetGuildRolesSummary()
    {
        var totalMembersWithRole = Users.Count(o => o.RoleIds.Any(x => x != Context.Guild.EveryoneRole.Id));
        var totalMembersWithoutRole = Users.Count(o => o.RoleIds.All(x => x == Context.Guild.EveryoneRole.Id));

        return Texts["Roles/GuildSummary", Locale].FormatWith(Context.Guild.Roles.Count, totalMembersWithRole, totalMembersWithoutRole);
    }

    private Embed CreateEmbed(IEnumerable<EmbedFieldBuilder> fields, Color color, string title, string summary = null)
    {
        var builder = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithTitle(title)
            .WithFields(fields);

        if (!string.IsNullOrEmpty(summary))
            builder = builder.WithDescription(summary);

        return builder.Build();
    }

    public async Task<Embed> ProcessDetailAsync(IRole role)
    {
        Users = await Context.Guild.GetUsersAsync();

        var fields = await GetDetailFieldsAsync(role);
        var title = Texts["Roles/DetailTitle", Locale].FormatWith(role.Name);
        return CreateEmbed(fields, role.Color, title);
    }

    private async Task<List<EmbedFieldBuilder>> GetDetailFieldsAsync(IRole role)
    {
        var result = new List<EmbedFieldBuilder>
        {
            CreateField("CreatedAt", role.CreatedAt.LocalDateTime.ToCzechFormat(), true),
            CreateField("Everyone", FormatHelper.FormatBoolean("Roles/Boolean", Locale, role.Id == Context.Guild.EveryoneRole.Id), true),
            CreateField("Hoisted", FormatHelper.FormatBoolean("Roles/Boolean", Locale, role.IsHoisted), true),
            CreateField("Managed", FormatHelper.FormatBoolean("Roles/Boolean", Locale, role.IsManaged), true),
            CreateField("Mentionable", FormatHelper.FormatBoolean("Roles/Boolean", Locale, role.IsMentionable), true)
        };

        if (role.Tags?.BotId == null)
        {
            var memberCount = Users.Count(o => o.RoleIds.Contains(role.Id));
            result.Add(CreateField("MemberCount", FormatHelper.FormatNumber("Roles/MemberCounts", Locale, memberCount), true));
        }

        if (role.Tags != null)
        {
            if (role.Tags.IsPremiumSubscriberRole)
                result.Add(CreateField("BoosterRole", FormatHelper.FormatBoolean("Roles/Boolean", Locale, true), true));

            if (role.Tags.BotId != null)
            {
                var botUser = await Context.Guild.GetUserAsync(role.Tags.BotId.Value);
                if (botUser != null)
                    result.Add(CreateField("BotUser", botUser.GetFullName(), false));
            }
        }

        var perms = role.Permissions.Administrator ? new List<string> { "Administrator" } : role.Permissions.ToList().ConvertAll(o => o.ToString());
        if (perms.Count > 0)
            result.Add(CreateField("Permissions", string.Join(", ", perms), false));

        return result;
    }

    private EmbedFieldBuilder CreateField(string fieldId, string value, bool inline)
        => new EmbedFieldBuilder().WithName(Texts[$"Roles/DetailFields/{fieldId}", Locale]).WithValue(value).WithIsInline(inline);
}
