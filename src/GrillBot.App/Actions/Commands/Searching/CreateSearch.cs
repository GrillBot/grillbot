using GrillBot.Database.Entity;

namespace GrillBot.App.Actions.Commands.Searching;

public class CreateSearch : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public CreateSearch(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(string message)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(Context.Guild);
        await repository.User.GetOrCreateUserAsync(Context.User);
        await repository.GuildUser.GetOrCreateGuildUserAsync(await GetExecutingUserAsync());
        await repository.Channel.GetOrCreateChannelAsync((IGuildChannel)Context.Channel);

        var entity = new SearchItem
        {
            ChannelId = Context.Channel.Id.ToString(),
            GuildId = Context.Guild.Id.ToString(),
            UserId = Context.User.Id.ToString(),
            MessageContent = message
        };

        await repository.AddAsync(entity);
        await repository.CommitAsync();
    }
}
