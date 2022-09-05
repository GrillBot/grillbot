using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Database.Models;

public class RangeParams<T> : IValidatableObject, IApiObject
{
    public T? From { get; set; }
    public T? To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From == null || To == null)
            yield break;

        var result = Comparer<T>.Default.Compare(From, To);
        if (result > 0)
            yield return new ValidationResult("Neplatný interval rozsahu Od-Do");
    }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(From), From?.ToString() ?? "" },
            { nameof(To), To?.ToString() ?? "" }
        };
    }
}
