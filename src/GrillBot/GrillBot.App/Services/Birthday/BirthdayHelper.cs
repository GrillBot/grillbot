namespace GrillBot.App.Services.Birthday;

public static class BirthdayHelper
{
    public static string Format(List<(IUser user, int? age)> users, IConfiguration configuration)
    {
        if (users.Count == 0)
        {
            return $"Dnes nemá nikdo narozeniny {configuration["Discord:Emotes:Sadge"]}";
        }

        var withoutLast = users.Take(users.Count - 1).Select(o => $"**{o.user.GetDisplayName(false)}{(o.age != null ? $" ({o.age} let)" : null)}**".Trim());
        var last = users[^1];

        var builder = new StringBuilder("Dnes ")
            .Append(users.Count == 1 ? "má" : "mají").Append(" narozeniny ")
            .AppendJoin(", ", withoutLast);

        if (users.Count > 1)
            builder.Append(" a ");

        builder.Append("**").Append(last.user.GetDisplayName(false));
        if (last.age != null)
            builder.Append(" (").Append(last.age).Append(" let)**");
        else
            builder.Append("**");

        builder.Append(' ').Append(configuration["Discord:Emotes:Hypers"]);
        return builder.ToString();
    }
}
