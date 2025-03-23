using GrillBot.App.Jobs.Abstractions;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class UserSynchronizationJob(
    GrillBotDatabaseBuilder _dbFactory,
    IServiceProvider serviceProvider
) : CleanerJobBase(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reportFields = new List<string>();

        using var repository = _dbFactory.CreateRepository();

        await ProcessOnlineUsersAsync(repository, reportFields);

        context.Result = FormatReportFromFields(reportFields);
    }

    private static async Task ProcessOnlineUsersAsync(GrillBotRepository repository, List<string> reportFields)
    {
        var users = await repository.User.GetOnlineUsersAsync();
        if (users.Count == 0)
            return;

        var privateUsersOnline = new List<string>();
        var publicUsersOnline = new List<string>();

        foreach (var user in users)
        {
            if (user.HaveFlags(UserFlags.PublicAdminOnline))
                publicUsersOnline.Add(user.GetDisplayName());
            if (user.HaveFlags(UserFlags.WebAdminOnline))
                privateUsersOnline.Add(user.GetDisplayName());

            user.Flags &= ~(int)UserFlags.WebAdminOnline;
            user.Flags &= ~(int)UserFlags.PublicAdminOnline;
        }

        await repository.CommitAsync();
        reportFields.Add(BuildReport(privateUsersOnline, publicUsersOnline, users.Count));
    }

    private static string BuildReport(IReadOnlyCollection<string> privateAdmin, IReadOnlyCollection<string> publicAdmin, int count)
    {
        var builder = new StringBuilder($"LoggedUsers (Count: {count})").AppendLine();

        if (privateAdmin.Count > 0)
            builder.Append("PrivateAdmin: ").AppendJoin(", ", privateAdmin).AppendLine();
        if (publicAdmin.Count > 0)
            builder.Append("PublicAdmin: ").AppendJoin(", ", publicAdmin).AppendLine();
        return builder.ToString().Trim();
    }
}
