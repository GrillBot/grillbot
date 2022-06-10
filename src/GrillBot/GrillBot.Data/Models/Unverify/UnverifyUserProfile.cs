using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Data.Models.Unverify;

public class UnverifyUserProfile
{
    public IGuildUser Destination { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<IRole> RolesToRemove { get; set; }
    public List<IRole> RolesToKeep { get; set; }
    public List<ChannelOverride> ChannelsToKeep { get; set; }
    public List<ChannelOverride> ChannelsToRemove { get; set; }
    public string Reason { get; set; }
    public bool IsSelfUnverify { get; set; }

    public UnverifyUserProfile(IGuildUser destination, DateTime start, DateTime end, bool isSelfUnverify)
    {
        Destination = destination;
        Start = start;
        End = end;
        IsSelfUnverify = isSelfUnverify;

        RolesToKeep = new List<IRole>();
        RolesToRemove = new List<IRole>();
        ChannelsToKeep = new List<ChannelOverride>();
        ChannelsToRemove = new List<ChannelOverride>();
    }

    public Task ReturnRolesAsync(RequestOptions options = null) 
        => Destination.AddRolesAsync(RolesToRemove, options);

    public async Task ReturnChannelsAsync(IGuild guild, RequestOptions options = null)
    {
        foreach (var channelToRemove in ChannelsToRemove)
        {
            var channel = await guild.GetChannelAsync(channelToRemove.ChannelId, options: options);
            if (channel == null) continue;

            await channel.AddPermissionOverwriteAsync(Destination, channelToRemove.Permissions, options);
        }
    }

    public Task RemoveRolesAsync(RequestOptions options = null) 
        => Destination.RemoveRolesAsync(RolesToRemove, options);

    public async Task RemoveChannelsAsync(SocketGuild guild, RequestOptions options = null)
    {
        var channels = ChannelsToRemove
            .Select(o => new { Channel = guild.GetChannel(o.ChannelId), Perms = o.Permissions })
            .Where(o => o.Channel != null);

        foreach (var channel in channels)
        {
            await channel.Channel.RemovePermissionOverwriteAsync(Destination, options);
        }
    }
}
