using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;

namespace GrillBot.App.Modules.Interactions;

[Group("message", "Message management")]
[RequireUserPerms]
public class MessageModule : InteractionsModuleBase
{
    public MessageModule(IServiceProvider provider) : base(provider)
    {
    }

    [Group("clear", "Removal process of message")]
    public class MessageClearSubModule : InteractionsModuleBase
    {
        public MessageClearSubModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [SlashCommand("react", "Remove emote from reactions.")]
        public async Task ClearEmoteFromReactionsAsync(IMessage message, IEmote emote)
        {
            await message.RemoveAllReactionsForEmoteAsync(emote);
            await SetResponseAsync(string.Format(GetText(nameof(ClearEmoteFromReactionsAsync), "Success"), emote));
        }
    }
}
