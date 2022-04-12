using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Common;

public class RangeParams<T> : IValidatableObject
{
    public T From { get; set; }
    public T To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From == null || To == null)
            yield break;

        var result = Comparer<T>.Default.Compare(From, To);
        if (result > 0)
            yield return new ValidationResult("Neplatný interval rozsahu Od-Do");
    }
}
