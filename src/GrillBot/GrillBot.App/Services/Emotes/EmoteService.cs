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
    private IMessageCacheManager MessageCache { get; }
    private IEmoteCache EmoteCache { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder, IMessageCacheManager messageCache, IEmoteCache emoteCache)
    {
        EmoteCache = emoteCache;
        MessageCache = messageCache;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.ReactionAdded += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Added);
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

        await repository.Guild.GetOrCreateGuildAsync(textChannel.Guild);
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
                break;
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

    #endregion

    #region GivenAndObtainedEmotes

    private static void Guild_OnReactionAdded(GuildUser reactingUserEntity, GuildUser messageAuthorEntity)
    {
        messageAuthorEntity.ObtainedReactions++;
        reactingUserEntity.GivenReactions++;
    }

    #endregion
}
