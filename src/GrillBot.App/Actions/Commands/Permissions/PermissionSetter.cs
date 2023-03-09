using GrillBot.Data.Models;

namespace GrillBot.App.Actions.Commands.Permissions;

public class PermissionSetter : CommandAction
{
    public Func<string, Task> OnProgress { get; set; } = _ => Task.CompletedTask;

    public async Task SetPermsToCategoryChannelsAsync(ICategoryChannel category, IRole role, bool viewChannel)
    {
        var permissions = new OverwritePermissions(viewChannel: viewChannel ? PermValue.Allow : PermValue.Deny);
        var channels = await FindChannelsAsync(category, role, permissions);
        var progressBar = new ProgressBar(channels.Count);
        var lastInvokedPrct = (int)Math.Round(progressBar.Percentage * 100);

        for (var i = 0; i < channels.Count; i++)
        {
            var channel = channels[i];
            var overwrite = channel.GetPermissionOverwrite(role);
            var allowValue = (overwrite?.AllowValue ?? 0) | permissions.AllowValue;
            var denyValue = (overwrite?.DenyValue ?? 0) | permissions.DenyValue;
            var newOverwrite = new OverwritePermissions(allowValue, denyValue);

            await channel.AddPermissionOverwriteAsync(role, newOverwrite);
            progressBar.SetValue(i + 1, $"**{i + 1}** / **{channels.Count}**");
            if (!progressBar.ValueChanged(lastInvokedPrct))
                continue;

            await OnProgress(progressBar.ToString());
            lastInvokedPrct = (int)Math.Round(progressBar.Percentage * 100);
        }

        await OnProgress(progressBar.ToString());
    }

    /// <summary>
    /// Find channels without role with specified permission.
    /// </summary>
    private async Task<List<IGuildChannel>> FindChannelsAsync(ICategoryChannel category, IRole role, OverwritePermissions permissions)
    {
        var channels = await Context.Guild.GetChannelsAsync();

        return channels
            .Where(o => o is INestedChannel nestedChannel && nestedChannel.CategoryId == category.Id)
            .Select(o => new { Overwrite = o.GetPermissionOverwrite(role), Channel = o })
            .Where(o => o.Overwrite is null || !(o.Overwrite.Value.AllowValue != permissions.AllowValue && o.Overwrite.Value.DenyValue != permissions.DenyValue))
            .Select(o => o.Channel)
            .ToList();
    }
}
