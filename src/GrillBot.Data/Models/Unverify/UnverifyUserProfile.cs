using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrillBot.Database.Entity;

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
    public string Language { get; set; }
    public bool KeepMutedRole { get; set; }

    public UnverifyUserProfile(IGuildUser destination, DateTime start, DateTime end, bool isSelfUnverify, string language)
    {
        Destination = destination;
        Start = start;
        End = end;
        IsSelfUnverify = isSelfUnverify;
        Language = language;

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

    public async Task RemoveChannelsAsync(IGuild guild, RequestOptions options = null)
    {
        foreach (var channelToRemove in ChannelsToRemove)
        {
            var channel = await guild.GetChannelAsync(channelToRemove.ChannelId);
            if (channel == null) continue;

            await channel.RemovePermissionOverwriteAsync(Destination, options);
        }
    }

    public Database.Entity.Unverify CreateRecord(IGuild guild, long logId)
    {
        return new Database.Entity.Unverify
        {
            Reason = Reason,
            Channels = ChannelsToRemove.ConvertAll(o => new GuildChannelOverride
            {
                ChannelId = o.ChannelId,
                AllowValue = o.AllowValue,
                DenyValue = o.DenyValue
            }),
            Roles = RolesToRemove.ConvertAll(o => o.Id.ToString()),
            EndAt = End,
            GuildId = guild.Id.ToString(),
            StartAt = Start,
            UserId = Destination.Id.ToString(),
            SetOperationId = logId
        };
    }
}
