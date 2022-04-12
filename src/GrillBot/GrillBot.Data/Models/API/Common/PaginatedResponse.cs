using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.Data.Models.API.Common
{
    /// <summary>
    /// Paginated result
    /// </summary>
    public class PaginatedResponse<TModel>
    {
        /// <summary>
        /// Data
        /// </summary>
        public List<TModel> Data { get; set; }

        /// <summary>
        /// Page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Total count of items.
        /// </summary>
        public long TotalItemsCount { get; set; }

        /// <summary>
        /// A flag that the user can request for next page.
        /// </summary>
        public bool CanNext { get; set; }

        /// <summary>
        /// A flag that the user can request for previous page.
        /// </summary>
        public bool CanPrev { get; set; }

        public static async Task<PaginatedResponse<TModel>> CreateAsync<TEntity>(IQueryable<TEntity> query, PaginatedParams @params,
            Converter<TEntity, TModel> itemConverter, CancellationToken cancellationToken)
        {
            var result = CreateEmpty(@params);
            result.TotalItemsCount = await query.CountAsync(cancellationToken);

            result.SetFlags(@params.Skip, @params.PageSize);

            if (result.TotalItemsCount == 0)
            {
                result.Data = new List<TModel>();
                return result;
            }

            query = query.Skip(@params.Skip).Take(@params.PageSize);
            result.Data = (await query.ToListAsync(cancellationToken)).ConvertAll(itemConverter);

            return result;
        }

        public static async Task<PaginatedResponse<TModel>> CreateAsync<TEntity>(IQueryable<TEntity> query, PaginatedParams @params,
            Func<TEntity, CancellationToken, Task<TModel>> asyncConverter, CancellationToken cancellationToken)
        {
            var result = CreateEmpty(@params);
            result.TotalItemsCount = await query.CountAsync(cancellationToken);
            result.SetFlags(@params.Skip, @params.PageSize);
            result.Data = new List<TModel>();

            if (result.TotalItemsCount == 0) return result;

            query = query.Skip(@params.Skip).Take(@params.PageSize);
            foreach (var item in await query.ToListAsync(cancellationToken))
            {
                result.Data.Add(await asyncConverter(item, cancellationToken));
            }

            return result;
        }

        public static PaginatedResponse<TModel> Create(List<TModel> data, PaginatedParams request)
        {
            var result = CreateEmpty(request);
            result.TotalItemsCount = data.Count;

            result.SetFlags(request.Skip, request.PageSize);
            result.Data = data.Skip(request.Skip).Take(request.PageSize).ToList();

            return result;
        }

        public static PaginatedResponse<TModel> CreateEmpty(PaginatedParams request)
        {
            if (request.Page <= 1)
                request.Page = 0;

            return new PaginatedResponse<TModel>()
            {
                Page = request.Page == 0 ? 1 : request.Page
            };
        }

        internal void SetFlags(int skip, int limit)
        {
            CanPrev = skip != 0;
            CanNext = skip + limit < TotalItemsCount;
        }
    }
}
