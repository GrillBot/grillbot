using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.MathJS;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    [Name("Matematické výpočty")]
    [Infrastructure.Preconditions.RequireUserPermission(new[] { ChannelPermission.SendMessages }, false)]
    public class MathModule : Infrastructure.ModuleBase
    {
        private IHttpClientFactory HttpClientFactory { get; }

        public MathModule(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [Command("solve")]
        [Summary("Spočítá matematický výraz pomocí MathJS API.")]
        public async Task SolveExpressionAsync([Remainder][Name("vyraz")] string expression)
        {
            var client = HttpClientFactory.CreateClient("MathJS");
            var request = new MathJSRequest() { Expression = expression };
            var requestJson = JsonConvert.SerializeObject(request);
            using var requestContent = new StringContent(requestJson);

            var embed = new EmbedBuilder()
               .WithFooter(Context.User.GetDisplayName(), Context.User.GetAvatarUri())
               .WithCurrentTimestamp()
               .AddField("Výraz", $"`{expression.Cut(EmbedFieldBuilder.MaxFieldValueLength)}`", false);

            try
            {
                using var response = await client.PostAsync("", requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                var calcResult = JsonConvert.DeserializeObject<MathJSResult>(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    embed.WithColor(Color.Red)
                        .WithTitle("Výpočet se nezdařil")
                        .AddField("Hlášení", calcResult.Error, false);
                }
                else
                {
                    embed.WithColor(Color.Green)
                        .AddField("Výsledek", calcResult.Result, false);
                }
            }
            catch (TaskCanceledException)
            {
                embed.WithColor(Color.Red)
                    .WithTitle("Vypršel časový limit pro zpracování výrazu");
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
