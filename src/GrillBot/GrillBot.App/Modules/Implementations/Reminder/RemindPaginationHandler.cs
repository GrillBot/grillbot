using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Reminder;

namespace GrillBot.App.Modules.Implementations.Reminder;

public class RemindPaginationHandler : ComponentInteractionHandler
{
    private DiscordSocketClient DiscordClient { get; }
    private RemindService RemindService { get; }
    private int Page { get; }

    public RemindPaginationHandler(RemindService remindService, DiscordSocketClient discordClient, int page)
    {
        RemindService = remindService;
        Page = page;
        DiscordClient = discordClient;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<RemindListMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var forUser = await DiscordClient.FindUserAsync(metadata.OfUser);
        if (forUser == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var remindsCount = await RemindService.GetRemindersCountAsync(forUser);
        if (remindsCount == 0)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var pagesCount = (int)Math.Ceiling(remindsCount / (double)EmbedBuilder.MaxFieldCount);
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var reminders = await RemindService.GetRemindersAsync(forUser, newPage);
        var result = await new EmbedBuilder()
            .WithRemindListAsync(reminders, DiscordClient, forUser, context.User, newPage);

        await component.UpdateAsync(msg => msg.Embed = result.Build());
    }
}
