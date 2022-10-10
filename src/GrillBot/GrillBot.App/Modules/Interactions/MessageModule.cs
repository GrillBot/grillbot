using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Modules.Interactions;

[Group("message", "Message management")]
[ExcludeFromCodeCoverage]
public class MessageModule : InteractionsModuleBase
{
    [Group("clear", "Removal process of message")]
    public class MessageClearSubModule : InteractionsModuleBase
    {
        public MessageClearSubModule(ITextsManager texts) : base(texts)
        {
        }

        [SlashCommand("react", "Remove emote from reactions.")]
        public async Task ClearEmoteFromReactionsAsync(IMessage message, IEmote emote)
        {
            await message.RemoveAllReactionsForEmoteAsync(emote);
            await SetResponseAsync(GetText(nameof(ClearEmoteFromReactionsAsync), "Success").FormatWith(emote));
        }
    }
}
