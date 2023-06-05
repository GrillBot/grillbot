using Discord.Interactions;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Common.Services.RubbergodService.Models.DirectApi;
using GrillBot.Data.Models.Rubbergod;

namespace GrillBot.App.Handlers.MessageReceived;

public class UnsucessCommandHandler : IMessageReceivedEvent
{
    private ITextsManager Texts { get; }
    private InteractionService InteractionService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private DataCacheManager DataCacheManager { get; }
    private Helpers.ChannelHelper ChannelHelper { get; }
    private IRubbergodServiceClient RubbergodServiceClient { get; }

    public UnsucessCommandHandler(ITextsManager texts, InteractionService interactionService, GrillBotDatabaseBuilder databaseBuilder, DataCacheManager dataCacheManager,
        Helpers.ChannelHelper channelHelper, IRubbergodServiceClient rubbergodServiceClient)
    {
        Texts = texts;
        InteractionService = interactionService;
        DatabaseBuilder = databaseBuilder;
        DataCacheManager = dataCacheManager;
        ChannelHelper = channelHelper;
        RubbergodServiceClient = rubbergodServiceClient;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!message.TryLoadMessage(out var userMessage) || userMessage == null || message.Channel is IDMChannel || !userMessage.Content.StartsWith('/')) return;

        var parts = message.Content[1..].Split(' ');

        var reference = new MessageReference(message.Id, message.Channel.Id, failIfNotExists: false);
        var commandMention = await FindLocalCommandMentionAsync(parts, message.Channel);
        if (string.IsNullOrEmpty(commandMention))
        {
            commandMention = await FindRubbergodCommandAsync(parts);
            if (string.IsNullOrEmpty(commandMention)) return;
        }

        var locale = await GetLastUserLocaleAsync(message.Author);
        var text = Texts["ClickOnCommand", locale] + (string.IsNullOrEmpty(commandMention) ? "" : $" ({commandMention})");
        await message.Channel.SendMessageAsync(text, messageReference: reference);
    }

    private async Task<string?> FindLocalCommandMentionAsync(IReadOnlyCollection<string> parts, IChannel channel)
    {
        var guild = await ChannelHelper.GetGuildFromChannelAsync(channel, channel.Id);
        if (guild is null) 
            return null;
        
        var commands = await InteractionService.RestClient.GetGuildApplicationCommands(guild.Id);
        var commandMentions = commands.GetCommandMentions();
        return TryMatchMention(commandMentions, parts);
    }

    private async Task<string?> FindRubbergodCommandAsync(IReadOnlyCollection<string> parts)
    {
        var commands = await DataCacheManager.GetValueAsync("RubbergodCommands");
        if (string.IsNullOrEmpty(commands) || commands == "{}")
        {
            var command = new DirectApiCommand("Help")
                .WithParameter("command", "slash_commands");
            commands = await RubbergodServiceClient.SendDirectApiCommand("Rubbergod", command);
            await DataCacheManager.SetValueAsync("RubbergodCommands", commands, DateTime.Now.AddDays(7));
        }

        var rubbergodCommands = JsonConvert.DeserializeObject<Dictionary<string, RubbergodCog?>>(commands)!;
        var cmdMentions = GetRubbergodCommandMentions(rubbergodCommands);
        return TryMatchMention(cmdMentions, parts);
    }

    private static string? TryMatchMention(IReadOnlyDictionary<string, string> mentions, IReadOnlyCollection<string> parts)
    {
        for (var i = 0; i < parts.Count; i++)
        {
            var command = string.Join(" ", parts.Take(i + 1));
            if (mentions.TryGetValue(command, out var mention))
                return mention;
        }

        return null;
    }

    private static Dictionary<string, string> GetRubbergodCommandMentions(Dictionary<string, RubbergodCog?> cogs)
    {
        var result = new Dictionary<string, string>();
        foreach (var cog in cogs.Where(o => o.Value?.Id != null))
        {
            if (cog.Value!.Children.Count == 0)
            {
                result.Add(cog.Key, $"</{cog.Key}:{cog.Value.Id}>");
                continue;
            }

            foreach (var child in cog.Value.Children)
                result.Add($"{cog.Key} {child}", $"</{cog.Key} {child}:{cog.Value.Id}>");
        }

        return result;
    }

    private async Task<string> GetLastUserLocaleAsync(IUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var userEntity = await repository.User.FindUserAsync(user, true);
        return userEntity?.Language ?? TextsManager.DefaultLocale;
    }
}
