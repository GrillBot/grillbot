using System.Text.RegularExpressions;
using GrillBot.Common.Extensions;
using GrillBot.Core.Extensions;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Managers;

public class AutoReplyManager
{
    private HashSet<ulong> DisabledChannels { get; set; }
    private List<AutoReplyItem> Messages { get; } = new();
    private SemaphoreSlim Semaphore { get; } = new(1);

    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public AutoReplyManager(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task InitAsync()
    {
        await Semaphore.WaitAsync();

        try
        {
            await using var repository = DatabaseBuilder.CreateRepository();
            var messages = await repository.AutoReply.GetAllAsync(true);

            Messages.Clear();
            Messages.AddRange(messages);

            var supportedChannelTypes = new List<ChannelType> { ChannelType.Stage, ChannelType.Text, ChannelType.Voice, ChannelType.PrivateThread, ChannelType.PublicThread };
            var disabledChannels = await repository.Channel.GetAllChannelsAsync(true, false, false, supportedChannelTypes);
            disabledChannels = disabledChannels.FindAll(o => o.HasFlag(ChannelFlag.AutoReplyDeactivated));

            DisabledChannels = disabledChannels.Select(o => o.ChannelId.ToUlong()).ToHashSet();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<bool> IsChannelDisabledAsync(ulong channelId)
    {
        await Semaphore.WaitAsync();

        try
        {
            return DisabledChannels.Contains(channelId);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<AutoReplyItem> FindAsync(string content)
    {
        await Semaphore.WaitAsync();

        try
        {
            return Messages
                .Find(o => Regex.IsMatch(content, o.Template, o.RegexOptions));
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
