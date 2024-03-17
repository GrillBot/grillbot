using System.Collections.Generic;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class StatisticsRepository : SubRepositoryBase<GrillBotContext>
{
    public StatisticsRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<Dictionary<string, int>> GetTablesStatusAsync()
    {
        using (CreateCounter())
        {
            return new Dictionary<string, int>
            {
                { nameof(Context.Users), await Context.Users.CountAsync() },
                { nameof(Context.Guilds), await Context.Guilds.CountAsync() },
                { nameof(Context.GuildUsers), await Context.GuildUsers.CountAsync() },
                { nameof(Context.Channels), await Context.Channels.CountAsync() },
                { nameof(Context.UserChannels), await Context.UserChannels.CountAsync() },
                { nameof(Context.Invites), await Context.Invites.CountAsync() },
                { nameof(Context.SearchItems), await Context.SearchItems.CountAsync() },
                { nameof(Context.Unverifies), await Context.Unverifies.CountAsync() },
                { nameof(Context.UnverifyLogs), await Context.UnverifyLogs.CountAsync() },
                { nameof(Context.Reminders), await Context.Reminders.CountAsync() },
                { nameof(Context.SelfunverifyKeepables), await Context.SelfunverifyKeepables.CountAsync() },
                { nameof(Context.AutoReplies), await Context.AutoReplies.CountAsync() },
                { nameof(Context.EmoteSuggestions), await Context.EmoteSuggestions.CountAsync() },
                { nameof(Context.ApiClients), await Context.ApiClients.CountAsync() },
                { nameof(Context.Nicknames), await Context.Nicknames.CountAsync() }
            };
        }
    }
}
