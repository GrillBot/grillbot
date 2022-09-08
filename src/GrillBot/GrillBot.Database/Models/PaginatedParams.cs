using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Common.Infrastructure;
using Newtonsoft.Json;
using NSwag.Annotations;

namespace GrillBot.Database.Models;

/// <summary>
/// Parameters for pagination.
/// </summary>
public class PaginatedParams : IApiObject
{
    /// <summary>
    /// Page.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Číslo stránky je v neplatném rozsahu.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Velikost stránky je v neplatném rozsahu.")]
    public int PageSize { get; set; } = 25;

    [OpenApiIgnore]
    [JsonIgnore]
    public int Skip => (Page == 0 ? 0 : Page - 1) * PageSize;

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(Page), Page.ToString() },
            { nameof(PageSize), PageSize.ToString() }
        };
    }
}
