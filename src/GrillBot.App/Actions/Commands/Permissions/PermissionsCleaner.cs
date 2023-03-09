using GrillBot.Data.Models;

namespace GrillBot.App.Actions.Commands.Permissions;

public class PermissionsCleaner : CommandAction
{
    public Func<string, Task> OnProgress { get; set; } = _ => Task.CompletedTask;

    public async Task ClearAllPermissionsAsync(IGuildChannel channel, IEnumerable<IGuildUser> excludedUsers)
    {
        var excludedUserIds = excludedUsers.Select(o => o.Id);
        var overwrites = channel.PermissionOverwrites
            .Where(o => o.TargetType == PermissionTarget.User && !excludedUserIds.Contains(o.TargetId))
            .ToList();

        await ClearPermissionsAsync(channel, overwrites);
    }

    private async Task ClearPermissionsAsync(IGuildChannel channel, IReadOnlyList<Overwrite> overwrites)
    {
        var progressBar = new ProgressBar(overwrites.Count);
        var lastInvokedPrct = (int)Math.Round(progressBar.Percentage * 100);

        for (var i = 0; i < overwrites.Count; i++)
        {
            var overwrite = overwrites[i];
            if (overwrite.TargetType == PermissionTarget.Role)
            {
                var role = channel.Guild.GetRole(overwrite.TargetId);
                await channel.RemovePermissionOverwriteAsync(role);
            }
            else
            {
                var user = await channel.Guild.GetUserAsync(overwrite.TargetId);
                await channel.RemovePermissionOverwriteAsync(user);
            }

            progressBar.SetValue(i + 1, $"**{i + 1}** / **{overwrites.Count}**");
            if (!progressBar.ValueChanged(lastInvokedPrct))
                continue;

            await OnProgress(progressBar.ToString());
            lastInvokedPrct = (int)Math.Round(progressBar.Percentage * 100);
        }

        await OnProgress(progressBar.ToString());
    }
}
