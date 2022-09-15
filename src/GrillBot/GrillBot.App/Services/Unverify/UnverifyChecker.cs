using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
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
    private ITextsManager Texts { get; }

    public UnverifyChecker(GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration, IWebHostEnvironment environment,
        ITextsManager texts)
    {
        DatabaseBuilder = databaseBuilder;
        Environment = environment;

        var unverifyConfig = configuration.GetSection("Unverify");
        UnverifyMinimalTime = TimeSpan.FromMinutes(unverifyConfig.GetValue<int>("MinimalTimes:Unverify"));
        SelfunverifyMinimalTime = TimeSpan.FromMinutes(unverifyConfig.GetValue<int>("MinimalTimes:Selfunverify"));
        MaxKeepAccessCount = unverifyConfig.GetValue<int>("MaxKeepAccessCount");
        Texts = texts;
    }

    public async Task ValidateUnverifyAsync(IGuildUser user, IGuild guild, bool selfunverify, DateTime end, int keeped, string locale)
    {
        if (keeped > MaxKeepAccessCount)
            throw new ValidationException(Texts["Unverify/Validation/KeepableCountExceeded", locale].FormatWith(MaxKeepAccessCount));

        if (guild.OwnerId == user.Id)
            throw new ValidationException(Texts["Unverify/Validation/GuildOwner", locale].FormatWith(user.GetDisplayName()));

        await using var repository = DatabaseBuilder.CreateRepository();
        var dbUser = await repository.GuildUser.GetOrCreateGuildUserAsync(user);

        if (!selfunverify)
        {
            if (!Environment.IsDevelopment() && (user.GuildPermissions.Administrator || dbUser.User!.HaveFlags(UserFlags.BotAdmin)))
                throw new ValidationException(Texts["Unverify/Validation/Administrator", locale].FormatWith(user.GetDisplayName()));

            await ValidateRolesAsync(guild, user, locale);
        }

        if (dbUser.Unverify != null)
            throw new ValidationException(Texts["Unverify/Validation/MultipleUnverify", locale].FormatWith(user.GetDisplayName(), dbUser.Unverify.EndAt.ToCzechFormat()));

        ValidateUnverifyDate(end, dbUser.User!.SelfUnverifyMinimalTime, selfunverify, locale);
    }

    public void ValidateUnverifyDate(DateTime end, TimeSpan? usersMinimalSelfUnverifyTime, bool selfunverify, string locale)
    {
        var diff = end - DateTime.Now.AddSeconds(-5); // Add 5 seconds tolerance.

        if (diff.TotalMinutes < 0)
            throw new ValidationException(Texts["Unverify/Validation/MustBeInFuture", locale]);

        var minimal = selfunverify ? usersMinimalSelfUnverifyTime ?? SelfunverifyMinimalTime : UnverifyMinimalTime;
        if (diff < minimal)
            throw new ValidationException(Texts["Unverify/Validation/MinimalTime", locale]
                .FormatWith(minimal.Humanize(culture: Texts.GetCulture(locale), precision: int.MaxValue, minUnit: TimeUnit.Second)));
    }

    private async Task ValidateRolesAsync(IGuild guild, IGuildUser user, string locale)
    {
        var currentUser = await guild.GetCurrentUserAsync();
        var botRolePosition = currentUser.GetRoles().Max(o => o.Position);

        var userRoles = user.GetRoles().ToList();
        var userMaxRolePosition = userRoles.Max(o => o.Position);

        if (userMaxRolePosition <= botRolePosition)
            return;

        var higherRoles = userRoles.Where(o => o.Position > botRolePosition).Select(o => o.Name);
        var higherRoleNames = string.Join(", ", higherRoles);

        throw new ValidationException(Texts["Unverify/Validation/HigherRoles", locale].FormatWith(user.GetDisplayName(), higherRoleNames));
    }
}
