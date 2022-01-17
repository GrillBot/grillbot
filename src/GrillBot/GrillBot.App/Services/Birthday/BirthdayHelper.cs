using Discord;
using GrillBot.Data.Extensions.Discord;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrillBot.Data.Services.Birthday
{
    public static class BirthdayHelper
    {
        public static string Format(List<Tuple<IUser, int?>> users, IConfiguration configuration)
        {
            if (users.Count == 0)
            {
                return $"Dnes nemá nikdo narozeniny {configuration["Discord:Emotes:Sadge"]}";
            }
            else
            {
                var withoutLast = users.Take(users.Count - 1).Select(o => $"**{o.Item1.GetDisplayName(false)}{(o.Item2 != null ? $" ({o.Item2} let)" : null)}**".Trim());
                var last = users[^1];

                var builder = new StringBuilder("Dnes ")
                    .Append(users.Count == 1 ? "má" : "mají").Append(" narozeniny ")
                    .AppendJoin(", ", withoutLast);

                if (users.Count > 1)
                    builder.Append(" a ");

                builder.Append("**").Append(last.Item1.GetDisplayName(false));
                if (last.Item2 != null)
                    builder.Append(" (").Append(last.Item2).Append(" let)**");
                else
                    builder.Append("**");

                builder.Append(' ').Append(configuration["Discord:Emotes:Hypers"]);
                return builder.ToString();
            }
        }
    }
}
