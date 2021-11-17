using Discord;
using Discord.Commands;
using GrillBot.App.Extensions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.App.Modules
{
    [Name("Mockování zpráv")]
    [Infrastructure.Preconditions.RequireUserPermission(new[] { ChannelPermission.SendMessages }, false)]
    public class MockingModule : Infrastructure.ModuleBase
    {
        private Emote MockingEmote { get; }
        private Random Random { get; }

        // MaxMessageSize - 2xMocking emotes - Spaces
        private int MaxMessageLength => DiscordConfig.MaxMessageSize - (2 * MockingEmote.ToString().Length) - 2;

        public MockingModule(IConfiguration configuration, RandomizationService randomization)
        {
            MockingEmote = Emote.Parse(configuration.GetValue<string>("Discord:Emotes:Mocking"));
            Random = randomization.GetOrCreateGenerator("Mocking");
        }

        // Get maximum range value for a random number generator that decides if the char should be uppercase.
        // When the char is uppercased, the index is set to last element.
        // The index is decremented for each lowercased char
        //
        // This means the char following uppercased char has 20% (1/5) chance of changing to uppercase.
        // If it's not changed, then the next char has 50% (1/2) chance of being uppercased. Finally if
        // even the second char is not uppercased, the next valid char has 100% chance.
        private readonly int[] MockRandomCoefficient = { 1, 2, 5 };

        private string CreateMockingString(string original)
        {
            original = original.ToLower().Cut(MaxMessageLength, true);

            var resultBuilder = new StringBuilder();
            var coeffIndex = 0;

            foreach (var c in original)
            {
                // Letter 'i' cannot be uppercased and letter 'l' should be always uppercased.
                // This feature is here to prevent confusion of lowercase 'l' and uppercase 'i'
                if (char.IsLetter(c) && c != 'i' && (c == 'l' || Random.Next(MockRandomCoefficient[coeffIndex]) == 0))
                {
                    resultBuilder.Append(char.ToUpperInvariant(c));
                    coeffIndex = MockRandomCoefficient.Length - 1;
                    continue;
                }

                resultBuilder.Append(c);

                if (coeffIndex > 0) coeffIndex--;
            }

            return $"{MockingEmote} {resultBuilder} {MockingEmote}";
        }

        [Command("mock")]
        [Summary("Mockuje existující zprávu (pro použití je třeba na cílovou zprávu tímto příkazem odpovědět).")]
        public async Task<RuntimeResult> MockAsync()
        {
            var referencedMsg = Context.Message.ReferencedMessage;

            if (referencedMsg == null)
            {
                await ReplyAsync("Chybí zpráva k mockování nebo odpověď na mockovanou zprávu.");
                return null;
            }

            // Easter egg. If user is mocking bot, send peepoangry instead
            if (Context.Message.ReferencedMessage.Author.Id == Context.Client.CurrentUser.Id)
                return new CommandRedirectResult($"angry {Context.User.Id}");

            var message = referencedMsg.ToString();

            // We are mocking referenced message. Reply to the author
            // of the original referenced message instead of replying to mocker
            await Context.Channel.SendMessageAsync(
                CreateMockingString(message),
                options: RequestOptions.Default,
                allowedMentions: new AllowedMentions() { MentionRepliedUser = true },
                messageReference: new MessageReference(Context.Message.ReferencedMessage.Id, Context.Channel.Id, Context.Guild?.Id)
            );

            return null;
        }

        [Command("mock")]
        [Summary("Mockuje zadanou zprávu.")]
        public Task MockAsync([Remainder][Name("zpráva")] string message) => ReplyAsync(CreateMockingString(message));
    }
}
