using GrillBot.App.Managers;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;

namespace GrillBot.App.Actions.Commands.Reminder;

public class CreateRemind : CommandAction
{
    private IConfiguration Configuration { get; }
    private FormatHelper FormatHelper { get; }

    private readonly IRemindServiceClient _remindServiceClient;
    private readonly UserManager _userManager;
    private readonly ITextsManager _texts;

    public CreateRemind(ITextsManager texts, IConfiguration configuration, FormatHelper formatHelper, IRemindServiceClient remindServiceClient, UserManager userManager)
    {
        Configuration = configuration;
        FormatHelper = formatHelper;
        _remindServiceClient = remindServiceClient;
        _userManager = userManager;
        _texts = texts;
    }

    private int MinimalTimeMinutes => Configuration.GetValue<int>("Reminder:MinimalTimeMinutes");
    private string MinimalTime => FormatHelper.FormatNumber("RemindModule/Create/Validation/MinimalTime", Locale, MinimalTimeMinutes);
    private string MinimalTimeTemplate => _texts["RemindModule/Create/Validation/MinimalTimeTemplate", Locale];

    public async Task<long> ProcessAsync(IUser from, IUser to, DateTime at, string message, ulong originalMessageId)
    {
        var request = new CreateReminderRequest
        {
            CommandMessageId = originalMessageId.ToString(),
            FromUserId = from.Id.ToString(),
            Language = (await _userManager.GetUserLanguage(to)) ?? Locale,
            Message = message,
            NotifyAtUtc = at.WithKind(DateTimeKind.Local).ToUniversalTime(),
            ToUserId = to.Id.ToString()
        };

        try
        {
            var result = await _remindServiceClient.CreateReminderAsync(request);
            return result.Id;
        }
        catch (ClientBadRequestException ex)
        {
            throw ProcessRemindServiceErrors(ex);
        }
    }

    private Exception ProcessRemindServiceErrors(ClientBadRequestException exception)
    {
        var firstError = exception.ValidationErrors.FirstOrDefault();

        if (firstError.Key == "NotifyAtUtc")
        {
            var minimalTime = Array.Find(firstError.Value, e => e.EndsWith("MinimalTime"));
            if (!string.IsNullOrEmpty(minimalTime))
                throw new ValidationException(MinimalTimeTemplate.FormatWith(MinimalTime));
        }

        return string.IsNullOrEmpty(firstError.Key) || firstError.Value.Length == 0
            ? exception
            : new ValidationException(_texts[firstError.Value[0], Locale]);
    }
}
