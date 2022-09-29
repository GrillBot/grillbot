using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.Common;
using GrillBot.Common.Helpers;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Name("Náhodné věci")]
[RequireUserPerms]
public class MemeModule : ModuleBase
{
    [Command("peepolove")]
    [Alias("love")]
    [TextCommandDeprecated(AlternativeCommand = "/peepolove")]
    public Task PeepoloveAsync(IUser user = null) => Task.CompletedTask;

    [Command("peepoangry")]
    [Alias("angry")]
    [TextCommandDeprecated(AlternativeCommand = "/peepoangry")]
    public Task PeepoangryAsync(IUser user = null) => Task.CompletedTask;

    [Command("kachna")]
    [Alias("duck")]
    [TextCommandDeprecated(AlternativeCommand = "/kachna")]
    public Task GetDuckInfoAsync() => Task.CompletedTask;

    [Command("hi")]
    [Summary("Pozdraví uživatele")]
    [TextCommandDeprecated(AlternativeCommand = "/hi")]
    public Task HiAsync(int? _ = null) => Task.CompletedTask; // Command was reimplemented to Slash command.

    #region Emojization

    [Command("emojize")]
    [Summary("Znovu pošle zprávu jako emoji.")]
    [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění mazat zprávy.")]
    public async Task EmojizeAsync([Remainder] [Name("zprava")] string message = null)
    {
        if (string.IsNullOrEmpty(message))
            message = Context.Message.ReferencedMessage?.Content;

        if (string.IsNullOrEmpty(message))
        {
            await ReplyAsync("Nemám zprávu, kterou můžu převést.");
            return;
        }

        var sanitized = MessageHelper.ClearEmotes(message, Context.Message.Tags.Where(o => o.Type == TagType.Emoji).Select(o => o.Value).OfType<IEmote>());
        if (string.IsNullOrEmpty(sanitized))
        {
            await ReplyAsync("Nelze převést zprávu, kterou tvoří pouze emoji.");
            return;
        }

        var emojized = Emojis.ConvertStringToEmoji(sanitized, true);
        if (emojized.Count == 0)
        {
            await ReplyAsync("Nepodařilo se převést zprávu na emoji.");
            return;
        }

        if (!Context.IsPrivate)
            await Context.Message.DeleteAsync();

        var messageBuilder = new StringBuilder();
        foreach (var emoji in emojized.Select(o => o.ToString()))
        {
            if (messageBuilder.Length + emoji.Length + 1 > DiscordConfig.MaxMessageSize)
                break;

            messageBuilder.Append(emoji).Append(' ');
        }
        
        await ReplyAsync(messageBuilder.ToString(), false);
    }

    [Command("reactjize")]
    [Summary("Převede zprávu na emoji a zapíše jako reakce na zprávu v reply.")]
    [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění na přidávání reakcí.")]
    [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění na mazání zpráv.")]
    public async Task ReactjizeAsync([Remainder] [Name("zprava")] string msg = null)
    {
        if (Context.Message.ReferencedMessage == null)
        {
            await ReplyAsync("Tento příkaz vyžaduje reply.");
            return;
        }

        if (string.IsNullOrEmpty(msg))
        {
            await ReplyAsync("Nelze vytvořit text z reakcí nad prázdnou zprávou.");
            return;
        }

        try
        {
            var emojis = Emojis.ConvertStringToEmoji(msg).Take(20).ToArray();
            if (emojis.Length == 0) return;

            await Context.Message.ReferencedMessage.AddReactionsAsync(emojis);
            await Context.Message.DeleteAsync();
        }
        catch (ArgumentException ex)
        {
            await ReplyAsync(ex.Message);
        }
    }

    #endregion
}
