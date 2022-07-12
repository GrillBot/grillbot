using Discord.Commands;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.Images;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Helpers;
using ModuleBase = GrillBot.App.Infrastructure.Commands.ModuleBase;

namespace GrillBot.App.Modules.TextBased;

[Name("Náhodné věci")]
[RequireUserPerms]
public class MemeModule : ModuleBase
{
    private FileStorageFactory FileStorageFactory { get; }
    private ProfilePictureManager ProfilePictureManager { get; }

    public MemeModule(FileStorageFactory fileStorage, ProfilePictureManager profilePictureManager)
    {
        FileStorageFactory = fileStorage;
        ProfilePictureManager = profilePictureManager;
    }

    #region Peepolove

    [Command("peepolove")]
    [Alias("love")]
    public async Task PeepoloveAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
    {
        user ??= Context.User;
        using var renderer = new PeepoloveRenderer(FileStorageFactory, ProfilePictureManager);
        var path = await renderer.RenderAsync(user, Context);

        await ReplyFileAsync(path, false);
    }

    #endregion

    #region Peepoangry

    [Command("peepoangry")]
    [Alias("angry")]
    [Summary("Naštvaně zírající peepo.")]
    public async Task PeepoangryAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
    {
        user ??= Context.User;
        using var renderer = new PeepoangryRenderer(FileStorageFactory, ProfilePictureManager);
        var path = await renderer.RenderAsync(user, Context);

        await ReplyFileAsync(path, false);
    }

    #endregion

    [Command("kachna")]
    [Alias("duck")]
    [TextCommandDeprecated(AlternativeCommand = "/kachna")]
    public Task GetDuckInfoAsync() => Task.CompletedTask;

    #region Hi

    [Command("hi")]
    [Summary("Pozdraví uživatele")]
    [TextCommandDeprecated(AlternativeCommand = "/hi")]
    public Task HiAsync(int? _ = null) => Task.CompletedTask; // Command was reimplemented to Slash command.

    #endregion

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
            var emojis = Emojis.ConvertStringToEmoji(msg);
            if (emojis.Count == 0) return;

            await Context.Message.ReferencedMessage.AddReactionsAsync(emojis.ToArray());
            await Context.Message.DeleteAsync();
        }
        catch (ArgumentException ex)
        {
            await ReplyAsync(ex.Message);
        }
    }

    #endregion
}
