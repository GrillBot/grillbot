using NSwag.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GrillBot.Data.Models.API.Params
{
    public class PaginatedParams<TEntity>
    {
        [Range(0, int.MaxValue, ErrorMessage = "Číslo stránky je v neplatném rozsahu.")]
        public int Page { get; set; } = 1;

        [Range(0, int.MaxValue, ErrorMessage = "Velikost stránky je v neplatném rozsahu.")]
        public int PageSize { get; set; } = 25;

        [OpenApiIgnore]
        public int Skip => (Page == 0 ? 0 : Page - 1) * PageSize;

        public virtual IQueryable<TEntity> CreateQuery(IQueryable<TEntity> query)
        {
            return query.Skip(Skip).Take(PageSize);
        }
    }
}
