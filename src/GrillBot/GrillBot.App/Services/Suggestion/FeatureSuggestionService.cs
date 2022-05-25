using GrillBot.App.Infrastructure;
using GrillBot.App.Modules.Implementations.Suggestion;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Enums;
using MailKit.Net.Smtp;
using MimeKit;

namespace GrillBot.App.Services.Suggestion;

public class FeatureSuggestionService : ServiceBase
{
    private SuggestionSessionService SessionService { get; }
    private IConfiguration Configuration { get; }

    public FeatureSuggestionService(SuggestionSessionService sessionService, IConfiguration configuration,
        GrillBotContextFactory dbFactory) : base(null, dbFactory, null, null)
    {
        SessionService = sessionService;
        Configuration = configuration.GetSection("Services:Trello");
    }

    public void InitSession(string suggestionId)
        => SessionService.InitSuggestion(suggestionId, SuggestionType.FeatureRequest, null);

    public async Task ProcessSessionAsync(string suggestionId, IGuild guild, IUser user, FeatureSuggestionModal modalData)
    {
        var metadata = SessionService.PopMetadata(SuggestionType.FeatureRequest, suggestionId);
        if (metadata == null)
            throw new NotFoundException("Nepodařilo se dohledat všechna data k tomuto návrhu. Podej prosím návrh znovu.");

        modalData.User = user.GetFullName();

        var entity = new Database.Entity.Suggestion()
        {
            CreatedAt = DateTime.Now,
            Data = JsonConvert.SerializeObject(modalData),
            Type = SuggestionType.FeatureRequest,
            GuildId = guild.Id.ToString()
        };

        await TrySendSuggestionAsync(entity);
    }

    public async Task TrySendSuggestionAsync(Database.Entity.Suggestion suggestion)
    {
        try
        {
            var modalData = JsonConvert.DeserializeObject<FeatureSuggestionModal>(suggestion.Data);

            var dataBuilder = new StringBuilder()
                .Append("Návrh od uživatele: **").Append(modalData.User).AppendLine("**")
                .AppendLine()
                .AppendLine(modalData.Description);

            var sender = new MailboxAddress("GrillBot", Configuration.GetValue<string>("Mail:Sender"));
            var receiver = new MailboxAddress("GrillBot-Suggestions", Configuration.GetValue<string>("BoardAddress"));
            var body = new BodyBuilder { TextBody = dataBuilder.ToString() };

            using var message = new MimeMessage(new[] { sender }, new[] { receiver }, modalData.Name, body.ToMessageBody());
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(Configuration.GetValue<string>("Mail:Server"), Configuration.GetValue<int>("Mail:Port"), true);
            await smtp.AuthenticateAsync(Configuration.GetValue<string>("Mail:Username"), Configuration.GetValue<string>("Mail:Password"));
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            if (ex is GrillBotException)
                return;

            if (suggestion.Id == default)
            {
                using var context = DbFactory.Create();

                await context.AddAsync(suggestion);
                await context.SaveChangesAsync();
            }

            throw;
        }
    }
}
