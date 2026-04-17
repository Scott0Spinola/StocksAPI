using Microsoft.EntityFrameworkCore;

namespace Pages.Services;

/// <summary>
/// Represents a single page of results along with paging metadata.
/// </summary>
/// <typeparam name="T">Type of the items returned in the page.</typeparam>
public class PagedList<T>(List<T> items, int page, int pageSize, int totalCount)
{
    /// <summary>
    /// Items in the requested page.
    /// </summary>
    public List<T> Items { get; } = items;

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; } = page;

    /// <summary>
    /// Maximum number of items per page.
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount { get; } = totalCount;

    /// <summary>
    /// True when there is at least one page after the current page.
    /// </summary>
    public bool HasNextPage => Page * PageSize < TotalCount;

    /// <summary>
    /// True when there is at least one page before the current page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Creates a <see cref="PagedList{T}"/> from an <see cref="IQueryable{T}"/> using database-side paging.
    /// </summary>
    /// <param name="query">Base query to page over.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>A populated <see cref="PagedList{T}"/>.</returns>
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedList<T>(items, page, pageSize, totalCount);
    }

    /// <summary>
    /// Creates a <see cref="PagedList{T}"/> from an in-memory <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="query">Sequence to page over.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <returns>A populated <see cref="PagedList{T}"/>.</returns>
    public static Task<PagedList<T>> Create(IEnumerable<T> query, int page, int pageSize)
    {
        var totalCount = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedList<T>(items, page, pageSize, totalCount));
    }
}
