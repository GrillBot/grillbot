using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.Emotes;

[Initializable]
public class EmoteService
{
    private string CommandPrefix { get; }
    private MessageCacheManager MessageCache { get; }
    private EmotesCacheService EmotesCacheService { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration,
        MessageCacheManager messageCache, EmotesCacheService emotesCacheService)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        EmotesCacheService = emotesCacheService;
        MessageCache = messageCache;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.MessageReceived += OnMessageReceivedAsync;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
        DiscordClient.ReactionAdded += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Added);
        DiscordClient.ReactionRemoved += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Removed);
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        var supportedEmotes = EmotesCacheService.GetSupportedEmotes().ConvertAll(o => o.Item1);

        if (supportedEmotes.Count == 0) return; // Ignore events when no supported emotes is available.
        if (!message.TryLoadMessage(out var msg)) return; // Ignore messages from bots.
        if (string.IsNullOrEmpty(message.Content)) return; // Ignore empty messages.
        if (msg.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return; // Ignore commands.
        if (msg.Channel is not SocketTextChannel textChannel) return; // Ignore DMs.

        var emotes = message.GetEmotesFromMessage(supportedEmotes).ToList();
        if (emotes.Count == 0) return;
        if (msg.Author is not IGuildUser guildUser) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var guild = await repository.Guild.GetOrCreateRepositoryAsync(textChannel.Guild);
        var user = await repository.GuildUser.GetOrCreateGuildUserAsync(guildUser);

        foreach (var emote in emotes)
        {
            var dbEmote = await repository.Emote.FindStatisticAsync(emote, guildUser, textChannel.Guild);

            if (dbEmote == null)
            {
                dbEmote = new EmoteStatisticItem
                {
                    EmoteId = emote.ToString(),
                    FirstOccurence = DateTime.Now,
                    Guild = guild,
                    User = user
                };

                await repository.AddAsync(dbEmote);
            }

            dbEmote.LastOccurence = DateTime.Now;
            dbEmote.UseCount++;
        }

        await repository.CommitAsync();
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        if (!messageChannel.HasValue || messageChannel.Value is not ITextChannel) return;

        var supportedEmotes = EmotesCacheService.GetSupportedEmotes();
        if (supportedEmotes.Count == 0) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, null, true);
        if (msg is not IUserMessage userMessage) return;
        if (userMessage.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return;

        var emotes = msg.GetEmotesFromMessage(supportedEmotes.ConvertAll(o => o.Item1)).ToList();

        if (emotes.Count == 0) return;
        if (msg.Author is not IGuildUser guildUser) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        if (!await repository.GuildUser.ExistsAsync(guildUser)) return;

        foreach (var emote in emotes)
        {
            var dbEmote = await repository.Emote.FindStatisticAsync(emote, guildUser, guildUser.Guild);
            if (dbEmote == null || dbEmote.UseCount == 0) continue;

            dbEmote.UseCount--;
        }

        await repository.CommitAsync();
    }

    private async Task OnReactionAsync(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction, ReactionEvents @event)
    {
        var supportedEmotes = EmotesCacheService.GetSupportedEmotes();

        if (!channel.HasValue || channel.Value is not SocketTextChannel textChannel) return;
        if (supportedEmotes.Count == 0) return;
        if (reaction.Emote is not Emote emote) return;
        if (!supportedEmotes.Any(o => o.Item1.IsEqual(emote))) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, channel.Value);
        var reactionUser = (reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId)) as IGuildUser;

        if (msg?.Author is not IGuildUser author || author.Id == reaction.UserId) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        if (reactionUser != null)
            await repository.GuildUser.GetOrCreateGuildUserAsync(reactionUser);
        await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        switch (@event)
        {
            case ReactionEvents.Added:
            {
                if (reactionUser != null)
                {
                    await EmoteStats_OnReactionAddedAsync(repository, reactionUser, emote, textChannel.Guild);
                    await Guild_OnReactionAddedAsync(repository, reactionUser, author);
                }

                break;
            }
            case ReactionEvents.Removed:
            {
                if (reactionUser != null)
                {
                    await EmoteStats_OnReactionRemovedAsync(repository, reactionUser, emote, textChannel.Guild);
                    await Guild_OnReactionRemovedAsync(repository, textChannel.Guild, reactionUser, author);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(@event), @event, null);
        }

        await repository.CommitAsync();
    }

    #region EmoteStats

    private static async Task EmoteStats_OnReactionAddedAsync(GrillBotRepository repository, IUser user, IEmote emote, IGuild guild)
    {
        var dbEmote = await repository.Emote.FindStatisticAsync(emote, user, guild);
        if (dbEmote == null)
        {
            dbEmote = new EmoteStatisticItem
            {
                EmoteId = emote.ToString() ?? throw new InvalidOperationException(),
                UserId = user.Id.ToString(),
                FirstOccurence = DateTime.Now,
                GuildId = guild.Id.ToString(),
            };

            await repository.AddAsync(dbEmote);
        }

        dbEmote.UseCount++;
        dbEmote.LastOccurence = DateTime.Now;
    }

    private static async Task EmoteStats_OnReactionRemovedAsync(GrillBotRepository repository, IGuildUser user, IEmote emote, IGuild guild)
    {
        if (!await repository.GuildUser.ExistsAsync(user)) return;

        var dbEmote = await repository.Emote.FindStatisticAsync(emote, user, guild);
        if (dbEmote == null || dbEmote.UseCount == 0) return;

        dbEmote.UseCount--;
    }

    #endregion

    #region GivenAndObtainedEmotes

    private static async Task Guild_OnReactionAddedAsync(GrillBotRepository repository, IGuildUser user, IGuildUser messageAuthor)
    {
        var reactingUser = await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        var authorUser = await repository.GuildUser.GetOrCreateGuildUserAsync(messageAuthor);

        authorUser.ObtainedReactions++;
        reactingUser.GivenReactions++;
    }

    private static async Task Guild_OnReactionRemovedAsync(GrillBotRepository repository, IGuild guild, IGuildUser user, IGuildUser messageAuthor)
    {
        if (!await repository.Guild.ExistsAsync(guild)) return;

        var reactingUser = await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        if (reactingUser.GivenReactions > 0)
            reactingUser.GivenReactions--;

        var authorUser = await repository.GuildUser.GetOrCreateGuildUserAsync(messageAuthor);
        if (authorUser.ObtainedReactions > 0)
            authorUser.ObtainedReactions--;
    }

    #endregion
}
