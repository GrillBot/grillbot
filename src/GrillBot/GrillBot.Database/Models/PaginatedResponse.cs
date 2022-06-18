using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Models;

/// <summary>
/// Paginated result
/// </summary>
public class PaginatedResponse<TModel>
{
    /// <summary>
    /// Data
    /// </summary>
    public List<TModel> Data { get; set; } = new();

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

    /// <summary>
    /// Create new paginated result with entities from database.
    /// </summary>
    public static async Task<PaginatedResponse<TEntity>> CreateWithEntityAsync<TEntity>(IQueryable<TEntity> query, PaginatedParams @params)
    {
        var result = CreateEmptyWithEntity<TEntity>(@params);

        result.TotalItemsCount = await query.CountAsync();
        result.SetFlags(@params.Skip, @params.PageSize);

        if (result.TotalItemsCount == 0)
            return result;

        query = query.Skip(@params.Skip).Take(@params.PageSize);
        result.Data.AddRange(await query.ToListAsync());

        return result;
    }

    /// <summary>
    /// Create new paginated result with mapped items based on existing paginated model.
    /// </summary>
    public static async Task<PaginatedResponse<TModel>> CopyAndMapAsync<TEntity>(PaginatedResponse<TEntity> resultWithEntity,
        Func<TEntity, Task<TModel>> converter)
    {
        var result = new PaginatedResponse<TModel>
        {
            Page = resultWithEntity.Page,
            CanNext = resultWithEntity.CanNext,
            CanPrev = resultWithEntity.CanPrev,
            TotalItemsCount = resultWithEntity.TotalItemsCount
        };

        foreach (var item in resultWithEntity.Data)
            result.Data.Add(await converter(item));

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

    private static PaginatedResponse<TModel> CreateEmpty(PaginatedParams request)
    {
        if (request.Page <= 1)
            request.Page = 0;

        return new PaginatedResponse<TModel>
        {
            Page = request.Page == 0 ? 1 : request.Page
        };
    }

    private static PaginatedResponse<TEntity> CreateEmptyWithEntity<TEntity>(PaginatedParams @params)
    {
        if (@params.Page <= 1)
            @params.Page = 0;

        return new PaginatedResponse<TEntity>
        {
            Page = @params.Page == 0 ? 1 : @params.Page
        };
    }

    private void SetFlags(int skip, int limit)
    {
        CanPrev = skip != 0;
        CanNext = skip + limit < TotalItemsCount;
    }
}
