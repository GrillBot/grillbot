using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Emotes;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.Emotes;

[Initializable]
public class EmoteService
{
    private string CommandPrefix { get; }
    private IMessageCacheManager MessageCache { get; }
    private IEmoteCache EmoteCache { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IConfiguration configuration,
        IMessageCacheManager messageCache, IEmoteCache emoteCache)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        EmoteCache = emoteCache;
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
        var supportedEmotes = EmoteCache.GetEmotes().ConvertAll(o => o.Emote);

        if (supportedEmotes.Count == 0) return; // Ignore events when no supported emotes is available.
        if (!message.TryLoadMessage(out var msg)) return; // Ignore messages from bots.
        if (string.IsNullOrEmpty(msg?.Content)) return; // Ignore empty messages.
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

        var supportedEmotes = EmoteCache.GetEmotes().ConvertAll(o => o.Emote);
        if (supportedEmotes.Count == 0) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, null, true);
        if (msg is not IUserMessage userMessage) return;
        if (userMessage.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return;

        var emotes = msg.GetEmotesFromMessage(supportedEmotes).ToList();

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
        var supportedEmotes = EmoteCache.GetEmotes();

        if (!channel.HasValue || channel.Value is not SocketTextChannel textChannel) return;
        if (supportedEmotes.Count == 0) return;
        if (reaction.Emote is not Emote emote) return;
        if (!supportedEmotes.Any(o => o.Emote.IsEqual(emote))) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, channel.Value);
        var reactionUser = (reaction.User.IsSpecified ? reaction.User.Value : textChannel.Guild.GetUser(reaction.UserId)) as IGuildUser;

        if (msg?.Author is not IGuildUser author || author.Id == reaction.UserId) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateRepositoryAsync(textChannel.Guild);
        await repository.User.GetOrCreateUserAsync(author);
        if (reactionUser != null)
            await repository.User.GetOrCreateUserAsync(reactionUser);

        var reactionUserEntity = reactionUser != null ? await repository.GuildUser.GetOrCreateGuildUserAsync(reactionUser) : null;
        var authorUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        switch (@event)
        {
            case ReactionEvents.Added:
            {
                if (reactionUser != null)
                {
                    await EmoteStats_OnReactionAddedAsync(repository, reactionUser, emote, textChannel.Guild);
                    Guild_OnReactionAdded(reactionUserEntity, authorUserEntity);
                }

                break;
            }
            case ReactionEvents.Removed:
            {
                if (reactionUser != null)
                {
                    await EmoteStats_OnReactionRemovedAsync(repository, reactionUser, emote, textChannel.Guild);
                    await Guild_OnReactionRemovedAsync(repository, textChannel.Guild, reactionUserEntity, authorUserEntity);
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

    private static void Guild_OnReactionAdded(GuildUser reactingUserEntity, GuildUser messageAuthorEntity)
    {
        messageAuthorEntity.ObtainedReactions++;
        reactingUserEntity.GivenReactions++;
    }

    private static async Task Guild_OnReactionRemovedAsync(GrillBotRepository repository, IGuild guild, GuildUser reactingUserEntity, GuildUser messageAuthorEntity)
    {
        if (!await repository.Guild.ExistsAsync(guild)) return;

        if (reactingUserEntity.GivenReactions > 0)
            reactingUserEntity.GivenReactions--;

        if (messageAuthorEntity.ObtainedReactions > 0)
            messageAuthorEntity.ObtainedReactions--;
    }

    #endregion
}
