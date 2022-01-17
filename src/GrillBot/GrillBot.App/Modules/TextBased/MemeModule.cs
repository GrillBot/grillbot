using Discord;
using Discord.Commands;
using GrillBot.Data.Extensions;
using GrillBot.Data.Services.FileStorage;
using GrillBot.Data.Services.Images;
using GrillBot.Data.Enums;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.Duck;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Data.Modules.TextBased;

[Name("Náhodné věci")]
[Infrastructure.Preconditions.RequireUserPermission(new[] { ChannelPermission.SendMessages }, false)]
public class MemeModule : Infrastructure.ModuleBase
{
    private FileStorageFactory FileStorageFactory { get; }
    private IHttpClientFactory HttpClientFactory { get; }
    private CultureInfo Culture { get; }
    private IConfiguration Configuration { get; }

    public MemeModule(FileStorageFactory fileStorage, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        FileStorageFactory = fileStorage;
        HttpClientFactory = httpClientFactory;
        Culture = new CultureInfo("cs-CZ");
        Configuration = configuration;
    }

    #region Peepolove

    [Command("peepolove")]
    [Alias("love")]
    public async Task PeepoloveAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
    {
        if (user == null) user = Context.User;
        using var renderer = new PeepoloveRenderer(FileStorageFactory);
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
        if (user == null) user = Context.User;
        using var renderer = new PeepoangryRenderer(FileStorageFactory);
        var path = await renderer.RenderAsync(user, Context);

        await ReplyFileAsync(path, false);
    }

    #endregion

    #region Duck

    [Command("kachna")]
    [Alias("duck")]
    [Summary("Zjistí stav kachny.")]
    public async Task GetDuckInfoAsync()
    {
        var client = HttpClientFactory.CreateClient("KachnaOnline");
        var response = await client.GetAsync("states/current/legacy");

        if (!response.IsSuccessStatusCode)
        {
            await ReplyAsync("Nepodařilo se zjistit stav kachny. Zkus to prosím později.");
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<CurrentState>(json);

        var embed = new EmbedBuilder()
            .WithAuthor("U Kachničky")
            .WithColor(Color.Gold)
            .WithCurrentTimestamp();

        var titleBuilder = new StringBuilder();

        switch (data.State)
        {
            case DuckState.Private:
            case DuckState.Closed:
                ProcessPrivateOrClosed(titleBuilder, data, embed);
                break;
            case DuckState.OpenBar:
                ProcessOpenBar(titleBuilder, data, embed);
                break;
            case DuckState.OpenChillzone:
                ProcessChillzone(titleBuilder, data, embed);
                break;
            case DuckState.OpenEvent:
                ProcessOpenEvent(titleBuilder, data);
                break;
        }

        await ReplyAsync(embed: embed.WithTitle(titleBuilder.ToString()).Build());
    }

    private void ProcessPrivateOrClosed(StringBuilder titleBuilder, CurrentState currentState, EmbedBuilder embedBuilder)
    {
        titleBuilder.AppendLine("Kachna je zavřená.");

        if (currentState.NextOpeningDateTime.HasValue)
        {
            FormatWithNextOpening(titleBuilder, currentState, embedBuilder);
            return;
        }

        if (currentState.NextOpeningDateTime.HasValue && currentState.State != DuckState.Private)
        {
            FormatWithNextOpeningNoPrivate(currentState, embedBuilder);
            return;
        }

        titleBuilder.Append("Další otvíračka není naplánovaná.");
        AddNoteToEmbed(embedBuilder, currentState.Note);
    }

    private void FormatWithNextOpening(StringBuilder titleBuilder, CurrentState currentState, EmbedBuilder embedBuilder)
    {
        var left = currentState.NextOpeningDateTime.Value - DateTime.Now;

        titleBuilder
            .Append("Další otvíračka bude za ")
            .Append(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute))
            .Append('.');

        AddNoteToEmbed(embedBuilder, currentState.Note);
    }

    static private void FormatWithNextOpeningNoPrivate(CurrentState currentState, EmbedBuilder embed)
    {
        if (string.IsNullOrEmpty(currentState.Note))
        {
            embed.AddField("A co dál?",
                            $"Další otvíračka není naplánovaná, ale tento stav má skončit {currentState.NextStateDateTime:dd. MM. v HH:mm}. Co bude pak, to nikdo neví.",
                            false);

            return;
        }

        AddNoteToEmbed(embed, currentState.Note, "A co dál?");
    }

    private void ProcessOpenBar(StringBuilder titleBuilder, CurrentState currentState, EmbedBuilder embedBuilder)
    {
        titleBuilder.Append("Kachna je otevřená!");
        embedBuilder.AddField("Otevřeno", currentState.LastChange.ToString("HH:mm"), true);

        if (currentState.ExpectedEnd.HasValue)
        {
            var left = currentState.ExpectedEnd.Value - DateTime.Now;

            titleBuilder.Append(" Do konce zbývá ").Append(left.Humanize(culture: Culture, precision: int.MaxValue, minUnit: TimeUnit.Minute)).Append('.');
            embedBuilder.AddField("Zavíráme", currentState.ExpectedEnd.Value.ToString("HH:mm"), true);
        }

        var enableBeers = Configuration.GetValue<bool>("IsKachnaOpen:EnableBeersOnTap");
        if (enableBeers && currentState.BeersOnTap?.Length > 0)
        {
            var beers = string.Join(Environment.NewLine, currentState.BeersOnTap);
            embedBuilder.AddField("Aktuálně na čepu", beers, false);
        }

        AddNoteToEmbed(embedBuilder, currentState.Note);
    }

    static private void ProcessChillzone(StringBuilder titleBuilder, CurrentState currentState, EmbedBuilder embedBuilder)
    {
        titleBuilder
            .Append("Kachna je otevřená v režimu chillzóna až do ")
            .AppendFormat("{0:HH:mm}", currentState.ExpectedEnd.Value)
            .Append('!');

        AddNoteToEmbed(embedBuilder, currentState.Note);
    }

    static private void ProcessOpenEvent(StringBuilder titleBuilder, CurrentState currentState)
    {
        titleBuilder
            .Append("V Kachně právě probíhá akce „")
            .Append(currentState.EventName)
            .Append("“.");
    }

    static private void AddNoteToEmbed(EmbedBuilder embed, string note, string title = "Poznámka")
    {
        if (!string.IsNullOrEmpty(note))
            embed.AddField(title, note, false);
    }

    #endregion

    #region Hi

    [Command("hi")]
    [Summary("Pozdraví uživatele")]
    public async Task HiAsync(int? @base = null)
    {
        var supportedBase = new[] { 2, 8, 16, (int?)null };
        if (!supportedBase.Contains(@base)) return;

        var emote = Configuration.GetValue<string>("Discord:Emotes:FeelsWowMan");
        var msg = $"Ahoj {Context.User.GetDisplayName()} {emote}";

        if (@base == null)
            await ReplyAsync(msg);
        else
            await ReplyAsync(string.Join(" ", msg.Select(o => Convert.ToString(o, @base.Value))));
    }

    #endregion

    #region Emojization

    [Command("emojize")]
    [Summary("Znovu pošle zprávu jako emoji.")]
    [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění mazat zprávy.")]
    public async Task EmojizeAsync([Remainder][Name("zprava")] string message = null)
    {
        if (string.IsNullOrEmpty(message))
            message = Context.Message.ReferencedMessage?.Content;

        if (string.IsNullOrEmpty(message))
        {
            await ReplyAsync("Nemám zprávu, kterou můžu převést.");
            return;
        }

        var emojized = Emojis.ConvertStringToEmoji(message, true);
        if (emojized.Count == 0)
        {
            await ReplyAsync("Nepodařilo se převést zprávu na emoji.");
            return;
        }

        if (!Context.IsPrivate)
            await Context.Message.DeleteAsync();
        await ReplyAsync(string.Join(" ", emojized.Select(o => o.ToString())), false, null, null, null, null);
    }

    [Command("reactjize")]
    [Summary("Převede zprávu na emoji a zapíše jako reakce na zprávu v reply.")]
    [RequireBotPermission(ChannelPermission.AddReactions, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění na přidávání reakcí.")]
    [RequireBotPermission(ChannelPermission.ManageMessages, ErrorMessage = "Nemohu provést tento příkaz, protože nemám oprávnění na mazání zpráv.")]
    public async Task ReactjizeAsync([Remainder][Name("zprava")] string msg = null)
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
            var emojis = Emojis.ConvertStringToEmoji(msg, false);
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
