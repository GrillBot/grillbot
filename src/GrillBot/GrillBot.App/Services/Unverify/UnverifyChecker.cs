using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Extensions;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App.Services.Unverify;

public class UnverifyChecker
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private TimeSpan UnverifyMinimalTime { get; }
    private TimeSpan SelfunverifyMinimalTime { get; }
    private int MaxKeepAccessCount { get; }
    private IWebHostEnvironment Environment { get; }

    public UnverifyChecker(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, IWebHostEnvironment environment)
    {
        DatabaseBuilder = databaseBuilder;
        Environment = environment;

        var unverifyConfig = configuration.GetSection("Unverify");
        UnverifyMinimalTime = TimeSpan.FromMinutes(unverifyConfig.GetValue<int>("MinimalTimes:Unverify"));
        SelfunverifyMinimalTime = TimeSpan.FromMinutes(unverifyConfig.GetValue<int>("MinimalTimes:Selfunverify"));
        MaxKeepAccessCount = unverifyConfig.GetValue<int>("MaxKeepAccessCount");
    }

    public async Task ValidateUnverifyAsync(IGuildUser user, IGuild guild, bool selfunverify, DateTime end, int keeped)
    {
        if (keeped > MaxKeepAccessCount)
            throw new ValidationException($"Nelze si ponechat více než {MaxKeepAccessCount} rolí a kanálů.");

        if (guild.OwnerId == user.Id)
            throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetDisplayName()}** je vlastník tohoto serveru.");

        await using var repository = DatabaseBuilder.CreateRepository();
        var dbUser = await repository.GuildUser.GetOrCreateGuildUserAsync(user);

        if (!selfunverify)
        {
            if (!Environment.IsDevelopment() && (user.GuildPermissions.Administrator || dbUser.User!.HaveFlags(UserFlags.BotAdmin)))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetDisplayName()}** má administrátorská oprávnění.");

            await ValidateRolesAsync(guild, user);
        }

        if (dbUser.Unverify != null)
            throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetDisplayName()}** již má odebraný přístup do **{dbUser.Unverify.EndAt.ToCzechFormat()}**.");

        ValidateUnverifyDate(end, dbUser.User!.SelfUnverifyMinimalTime, selfunverify);
    }

    public void ValidateUnverifyDate(DateTime end, TimeSpan? usersMinimalSelfUnverifyTime, bool selfunverify)
    {
        var diff = end - DateTime.Now.AddSeconds(-5); // Add 5 seconds tolerance.

        if (diff.TotalMinutes < 0)
            throw new ValidationException("Konec unverify musí být v budoucnosti.");

        var minimal = selfunverify ? usersMinimalSelfUnverifyTime ?? SelfunverifyMinimalTime : UnverifyMinimalTime;
        if (diff < minimal)
            throw new ValidationException($"Minimální čas pro unverify je {minimal.Humanize(culture: new CultureInfo("cs-CZ"), precision: int.MaxValue, minUnit: TimeUnit.Second)}");
    }

    private static async Task ValidateRolesAsync(IGuild guild, IGuildUser user)
    {
        var currentUser = await guild.GetCurrentUserAsync();
        var botRolePosition = currentUser.GetRoles().Max(o => o.Position);

        var userRoles = user.GetRoles().ToList();
        var userMaxRolePosition = userRoles.Max(o => o.Position);

        if (userMaxRolePosition <= botRolePosition)
            return;

        var higherRoles = userRoles.Where(o => o.Position > botRolePosition).Select(o => o.Name);
        var higherRoleNames = string.Join(", ", higherRoles);

        throw new ValidationException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetFullName()}** má vyšší role. **({higherRoleNames})**");
    }
}
