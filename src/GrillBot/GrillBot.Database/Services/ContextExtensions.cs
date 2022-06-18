using Discord;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Database.Services;

public static class ContextExtensions
{
    public static async Task InitUserAsync(this GrillBotContext context, IUser? user, CancellationToken cancellationToken = default)
    {
        if (user == null) return;
        var userId = user.Id.ToString();

        if (!await context.Users.AnyAsync(o => o.Id == userId, cancellationToken))
            await context.AddAsync(User.FromDiscord(user), cancellationToken);
    }
}
