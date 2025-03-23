using GrillBot.App.Infrastructure.Jobs;

namespace GrillBot.App.Jobs.Abstractions;

public abstract class CleanerJobBase(IServiceProvider serviceProvider) : Job(serviceProvider)
{
    protected string? FormatReportFromFields(List<string> reportFields)
    {
        if (reportFields.Count == 0)
            return null;

        var jobName = GetJobName();
        var builder = new StringBuilder()
            .Append(jobName).AppendLine("(");

        foreach (var field in reportFields.Select(o => o.Split(Environment.NewLine)))
        {
            foreach (var line in field)
                builder.Append(Indent).AppendLine(line);
        }

        builder.Append(')');

        return builder.ToString();
    }

    private string GetJobName()
    {
        const string suffix = "Job";

        var typeName = GetType().Name;
        if (!typeName.EndsWith(suffix))
            return typeName;

        return typeName[..^suffix.Length];
    }
}
