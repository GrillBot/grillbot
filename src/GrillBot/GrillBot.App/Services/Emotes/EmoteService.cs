using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Emotes;

[Initializable]
public partial class EmoteService : ServiceBase
{
    private string CommandPrefix { get; }
    private MessageCacheManager MessageCache { get; }
    private EmotesCacheService EmotesCacheService { get; }

    public EmoteService(DiscordSocketClient client, GrillBotContextFactory dbFactory, IConfiguration configuration,
        MessageCacheManager messageCache, EmotesCacheService emotesCacheService) : base(client, dbFactory)
    {
        CommandPrefix = configuration.GetValue<string>("Discord:Commands:Prefix");
        EmotesCacheService = emotesCacheService;
        MessageCache = messageCache;

        DiscordClient.MessageReceived += OnMessageReceivedAsync;
        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
        DiscordClient.ReactionAdded += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Added);
        DiscordClient.ReactionRemoved += (msg, channel, reaction) => OnReactionAsync(msg, channel, reaction, ReactionEvents.Removed);
    }

    private async Task OnMessageReceivedAsync(SocketMessage message)
    {
        var supportedEmotes = EmotesCacheService.GetSupportedEmotes().ConvertAll(o => o.Item1);

        if (supportedEmotes.Count == 0) return; // Ignore events when no supported emotes is available.
        if (!message.TryLoadMessage(out SocketUserMessage msg)) return; // Ignore messages from bots.
        if (string.IsNullOrEmpty(message.Content)) return; // Ignore empty messages.
        if (msg.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return; // Ignore commands.
        if (msg.Channel is not SocketTextChannel textChannel) return; // Ignore DMs.

        var emotes = message.GetEmotesFromMessage(supportedEmotes).ToList();
        if (emotes.Count == 0) return;

        var userId = message.Author.Id.ToString();
        var guildId = textChannel.Guild.Id.ToString();

        using var context = DbFactory.Create();
        await context.InitGuildAsync(textChannel.Guild, CancellationToken.None);
        await context.InitUserAsync(message.Author, CancellationToken.None);

        foreach (var emote in emotes)
        {
            var emoteId = emote.ToString();
            var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.UserId == userId && o.EmoteId == emoteId && o.GuildId == guildId);

            if (dbEmote == null)
            {
                dbEmote = new EmoteStatisticItem()
                {
                    EmoteId = emoteId,
                    FirstOccurence = DateTime.Now,
                    UserId = userId,
                    GuildId = guildId,
                };

                await context.AddAsync(dbEmote);
            }

            dbEmote.LastOccurence = DateTime.Now;
            dbEmote.UseCount++;
        }

        await context.SaveChangesAsync();
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        if (!messageChannel.HasValue || messageChannel.Value is not SocketTextChannel textChannel) return;

        var supportedEmotes = EmotesCacheService.GetSupportedEmotes();
        if (supportedEmotes.Count == 0) return;

        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, null, true);
        if (msg is not IUserMessage userMessage) return;
        if (userMessage.IsCommand(DiscordClient.CurrentUser, CommandPrefix)) return;

        var emotes = msg.GetEmotesFromMessage(supportedEmotes.ConvertAll(o => o.Item1)).ToList();
        if (emotes.Count == 0) return;

        var userId = msg.Author.Id.ToString();
        var guildId = textChannel.Guild.Id.ToString();

        using var context = DbFactory.Create();
        if (!await context.GuildUsers.AsQueryable().AnyAsync(o => o.UserId == userId && o.GuildId == guildId)) return;

        foreach (var emote in emotes)
        {
            var emoteId = emote.ToString();
            var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.EmoteId == emoteId && o.UserId == userId && o.GuildId == guildId);
            if (dbEmote == null || dbEmote.UseCount == 0) continue;

            dbEmote.UseCount--;
        }

        await context.SaveChangesAsync();
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

        if (msg == null) return;
        if (msg.Author is not IGuildUser author || author.Id == reaction.UserId) return;

        using var context = DbFactory.Create();

        await context.InitUserAsync(reactionUser, CancellationToken.None);
        await context.InitUserAsync(msg.Author, CancellationToken.None);
        await context.InitGuildAsync(author.Guild, CancellationToken.None);

        if (@event == ReactionEvents.Added)
        {
            await EmoteStats_OnReactionAddedAsync(context, reaction.UserId, emote, textChannel.Guild);

            if (reactionUser != null)
                await Guild_OnReactionAddedAsync(context, textChannel.Guild, reactionUser, author);
        }
        else if (@event == ReactionEvents.Removed)
        {
            await EmoteStats_OnReactionRemovedAsync(context, reaction.UserId, emote, textChannel.Guild);

            if (reactionUser != null)
                await Guild_OnReactionRemovedAsync(context, textChannel.Guild, reactionUser, msg.Author);
        }

        await context.SaveChangesAsync();
    }

    #region EmoteStats

    private static async Task EmoteStats_OnReactionAddedAsync(GrillBotContext context, ulong userId, Emote emote, IGuild guild)
    {
        var strUserId = userId.ToString();
        var emoteId = emote.ToString();
        var guildId = guild.Id.ToString();

        var dbEmote = await context.Emotes.AsQueryable()
            .FirstOrDefaultAsync(o => o.UserId == strUserId && o.EmoteId == emoteId && o.GuildId == guildId);
        if (dbEmote == null)
        {
            dbEmote = new EmoteStatisticItem()
            {
                EmoteId = emoteId,
                UserId = strUserId,
                FirstOccurence = DateTime.Now,
                GuildId = guildId,
            };

            await context.AddAsync(dbEmote);
        }

        dbEmote.UseCount++;
        dbEmote.LastOccurence = DateTime.Now;
        await context.SaveChangesAsync();
    }

    private static async Task EmoteStats_OnReactionRemovedAsync(GrillBotContext context, ulong userId, Emote emote, IGuild guild)
    {
        var strUserId = userId.ToString();
        var emoteId = emote.ToString();
        var guildId = guild.Id.ToString();

        if (!await context.GuildUsers.AsQueryable().AnyAsync(o => o.UserId == strUserId && o.GuildId == guildId)) return;

        var dbEmote = await context.Emotes.AsQueryable().FirstOrDefaultAsync(o => o.UserId == strUserId && o.EmoteId == emoteId && o.GuildId == guildId);
        if (dbEmote == null || dbEmote.UseCount == 0) return;

        dbEmote.UseCount--;
        await context.SaveChangesAsync();
    }

    #endregion

    #region GivenAndObtainedEmotes

    private static async Task Guild_OnReactionAddedAsync(GrillBotContext context, SocketGuild guild, IGuildUser user, IGuildUser messageAuthor)
    {
        var guildId = guild.Id.ToString();
        var authorUserId = messageAuthor.Id.ToString();

        await context.InitGuildAsync(guild, CancellationToken.None);
        var reactingUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guild.Id.ToString() && o.UserId == user.Id.ToString());
        if (reactingUser == null)
        {
            reactingUser = GuildUser.FromDiscord(guild, user);
            await context.AddAsync(reactingUser);
        }

        var authorUser = await context.GuildUsers.AsQueryable()
            .FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == authorUserId);
        if (authorUser == null)
        {
            authorUser = GuildUser.FromDiscord(guild, messageAuthor);
            await context.AddAsync(authorUser);
        }

        authorUser.ObtainedReactions++;
        reactingUser.GivenReactions++;
    }

    private static async Task Guild_OnReactionRemovedAsync(GrillBotContext context, SocketGuild guild, IUser user, IUser messageAuthor)
    {
        var guildId = guild.Id.ToString();
        var userId = user.Id.ToString();
        var authorUserId = messageAuthor.Id.ToString();

        if (!await context.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId)) return;
        if (!await context.Users.AsQueryable().AnyAsync(o => o.Id == userId) && !await context.Users.AsQueryable().AnyAsync(o => o.Id == authorUserId)) return;

        var reactingUser = await context.GuildUsers.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == userId);
        if (reactingUser?.GivenReactions > 0)
            reactingUser.GivenReactions--;

        var authorUser = await context.GuildUsers.AsQueryable().FirstOrDefaultAsync(o => o.GuildId == guildId && o.UserId == authorUserId);
        if (authorUser?.ObtainedReactions > 0)
            authorUser.ObtainedReactions--;
    }

    #endregion
}
