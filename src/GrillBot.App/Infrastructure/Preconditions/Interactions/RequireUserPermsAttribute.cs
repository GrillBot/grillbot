using Discord.Interactions;
using GrillBot.Common.Managers.Localization;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Preconditions.Interactions;

public class RequireUserPermsAttribute : PreconditionAttribute
{
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var texts = services.GetRequiredService<ITextsManager>();
        var databaseBuilder = services.GetRequiredService<GrillBotDatabaseBuilder>();
        var locale = context.Interaction.UserLocale;

        await using var repository = databaseBuilder.CreateRepository();

        try
        {
            CheckDms(context, texts, commandInfo);

            var user = (await repository.User.FindUserAsync(context.User, true))!;
            if (user.HaveFlags(UserFlags.BotAdmin)) return PreconditionResult.FromSuccess();

            CheckUserDisabled(user, texts, locale);
            await CheckChannelAsync(repository, (IGuildChannel)context.Channel, texts, locale);

            return PreconditionResult.FromSuccess();
        }
        catch (UnauthorizedAccessException ex)
        {
            return PreconditionResult.FromError(ex.Message);
        }
    }

    private static void CheckDms(IInteractionContext context, ITextsManager texts, ICommandInfo commandInfo)
    {
        if (context.Interaction.IsDMInteraction && !commandInfo.Attributes.OfType<AllowDmsAttribute>().Any())
            throw new UnauthorizedAccessException(texts["Permissions/Preconditions/DmNotAllowed", context.Interaction.UserLocale]);
    }

    private static void CheckUserDisabled(Database.Entity.User user, ITextsManager texts, string locale)
    {
        if (user.HaveFlags(UserFlags.CommandsDisabled))
            throw new UnauthorizedAccessException(texts["Permissions/Preconditions/UserCommandsDisabled", locale]);
    }

    private static async Task CheckChannelAsync(GrillBotRepository repository, IGuildChannel channel, ITextsManager texts, string locale)
    {
        var channelData = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.GuildId, true);
        if (channelData == null || channelData.HasFlag(ChannelFlag.CommandsDisabled))
            throw new UnauthorizedAccessException(texts["Permissions/Preconditions/ChannelDisabled", locale]);
    }
}
