using Discord;
using Discord.Commands;
using GrapeCity.Documents.Imaging;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Services.FileStorage;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Duck;
using GrillBot.Data.Resources.Peepoangry;
using GrillBot.Data.Resources.Peepolove;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SysDraw = System.Drawing;

namespace GrillBot.App.Modules
{
    [Name("Náhodné věci")]
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
            var cache = FileStorageFactory.CreateCache();

            var filename = user.CreateProfilePicFilename(256);
            var file = await cache.GetFileInfoAsync("Peepolove", filename);
            bool withException = false;

            try
            {
                if (file.Exists) return;

                var profileImageInfo = await cache.GetProfilePictureInfoAsync(filename);
                if (!profileImageInfo.Exists)
                    await cache.StoreProfilePictureAsync(filename, await user.DownloadAvatarAsync(size: 256));

                if (file.Extension == ".gif" && profileImageInfo.Length > 2 * ((Context.Guild.CalculateFileUploadLimit() * 1024 * 1024) / 3))
                {
                    filename = Path.ChangeExtension(filename, ".png");
                    file = await cache.GetFileInfoAsync("Peepolove", filename);
                    if (file.Exists) return;
                }

                using var profilePictureImage = SysDraw.Image.FromFile(profileImageInfo.FullName);

                if (file.Extension == ".gif")
                {
                    using var gcBitmap = new GcBitmap();
                    using var gifWriter = new GcGifWriter(file.FullName);
                    var delay = profilePictureImage.CalculateGifDelay();

                    foreach (var profilePictureFrame in profilePictureImage.SplitGifIntoFrames())
                    {
                        try
                        {
                            using var rounded = profilePictureFrame.RoundImage();
                            using var frame = RenderPeepoloveFrame(rounded);

                            using var ms = new MemoryStream();
                            frame.Save(ms, SysDraw.Imaging.ImageFormat.Png);

                            gcBitmap.Load(ms.ToArray());
                            gifWriter.AppendFrame(gcBitmap, disposalMethod: GifDisposalMethod.RestoreToBackgroundColor, delayTime: delay);
                        }
                        finally
                        {
                            profilePictureFrame.Dispose();
                        }
                    }
                }
                else if (file.Extension == ".png")
                {
                    using var rounded = profilePictureImage.RoundImage();
                    using var resized = rounded.ResizeImage(256, 256);

                    using var frame = RenderPeepoloveFrame(resized);
                    frame.Save(file.FullName, SysDraw.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception) { withException = true; throw; }
            finally
            {
                if (File.Exists(file.FullName) && !withException)
                    await ReplyFileAsync(file.FullName, false);
            }
        }

        private static SysDraw.Image RenderPeepoloveFrame(SysDraw.Image profilePicture)
        {
            using var ms = new MemoryStream(PeepoloveResources.Body);
            using var body = new Bitmap(ms);
            using var graphics = Graphics.FromImage(body);

            graphics.DrawImage(profilePicture, new Rectangle(5, 312, 180, 180));

            using var handsStream = new MemoryStream(PeepoloveResources.Hands);
            using var hands = new Bitmap(handsStream);
            graphics.DrawImage(hands, new Rectangle(0, 0, 512, 512));

            return (body as SysDraw.Image).CropImage(new Rectangle(0, 115, 512, 397));
        }

        #endregion

        #region Peepoangry

        [Command("peepoangry")]
        [Alias("angry")]
        [Summary("Naštvaně zírající peepo.")]
        public async Task PeepoangryAsync([Name("id/tag/jmeno_uzivatele")] IUser user = null)
        {
            if (user == null) user = Context.User;
            var cache = FileStorageFactory.CreateCache();

            var filename = user.CreateProfilePicFilename(64);
            var file = await cache.GetFileInfoAsync("Peepoangry", filename);
            bool withException = false;

            try
            {
                if (file.Exists) return;

                var profilePictureImage = await cache.GetProfilePictureInfoAsync(filename);
                if (!profilePictureImage.Exists)
                    await cache.StoreProfilePictureAsync(filename, await user.DownloadAvatarAsync(size: 64));

                if (file.Extension == ".gif" && profilePictureImage.Length > 2 * ((Context.Guild.CalculateFileUploadLimit() * 1024 * 1024) / 3))
                {
                    filename = Path.ChangeExtension(filename, ".png");
                    file = await cache.GetFileInfoAsync("Peepoangry", filename);
                    if (file.Exists) return;
                }

                using var profilePicture = SysDraw.Image.FromFile(profilePictureImage.FullName);
                if (file.Extension == ".gif")
                {
                    using var gifWriter = new GcGifWriter(file.FullName);
                    using var bitmap = new GcBitmap();
                    var delayTime = profilePicture.CalculateGifDelay();

                    foreach (var userFrame in profilePicture.SplitGifIntoFrames())
                    {
                        try
                        {
                            using var roundedUserFrame = userFrame.RoundImage();
                            using var frame = RenderPeepoangryFrame(roundedUserFrame);

                            using var ms = new MemoryStream();
                            frame.Save(ms, SysDraw.Imaging.ImageFormat.Png);

                            bitmap.Load(ms.ToArray());
                            gifWriter.AppendFrame(bitmap, disposalMethod: GifDisposalMethod.RestoreToBackgroundColor, delayTime: delayTime);
                        }
                        finally
                        {
                            userFrame.Dispose();
                        }
                    }
                }
                else if (file.Extension == ".png")
                {
                    using var rounded = profilePicture.RoundImage();
                    using var resized = rounded.ResizeImage(64, 64);

                    using var frame = RenderPeepoangryFrame(resized);
                    frame.Save(file.FullName, SysDraw.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception) { withException = true; throw; }
            finally
            {
                if (File.Exists(file.FullName) && !withException)
                    await ReplyFileAsync(file.FullName, false);
            }
        }

        private static SysDraw.Image RenderPeepoangryFrame(SysDraw.Image profilePicture)
        {
            using var peepoangryStream = new MemoryStream(PeepoangryResources.peepoangry);

            var body = new Bitmap(250, 105);
            using var graphics = Graphics.FromImage(body);

            graphics.DrawImage(profilePicture, new Rectangle(20, 10, 64, 64));
            using var peepoangry = new Bitmap(peepoangryStream);
            graphics.DrawImage(peepoangry, new Point(115, -5));

            return body;
        }

        #endregion

        #region Duck

        [Command("kachna")]
        [Alias("duck")]
        [Summary("Zjistí stav kachny.")]
        public async Task GetDuckInfoAsync()
        {
            var client = HttpClientFactory.CreateClient("IsKachnaOpen");
            var response = await client.GetAsync("duck/currentState");

            if (!response.IsSuccessStatusCode)
            {
                await ReplyAsync("Nepodařilo se zjistit stav kachny. Zkus to prosím později.");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<CurrentState>(json);

            var embed = new EmbedBuilder()
                .WithAuthor("U kachničky")
                .WithColor(Discord.Color.Gold)
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
                .Append("Do další otvíračky zbývá ")
                .Append(left.Humanize(culture: Culture))
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

                titleBuilder.Append(" Do konce zbývá ").Append(left.Humanize(culture: Culture)).Append('.');
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
    }
}
