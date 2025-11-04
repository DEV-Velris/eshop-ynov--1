using System.Text.Json.Serialization;

namespace BuildingBlocks.Pagination;

/// <summary>
/// Represents a paginated result for a collection of data, providing
/// the relevant pagination information such as page index, page size,
/// total count of items, and the data for the specific page.
/// </summary>
/// <typeparam name="TEntity">
/// The type of the entities within the paginated result. Must be a reference type.
/// </typeparam>
public class PaginatedResult<TEntity> where TEntity : class
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public IEnumerable<TEntity> Data { get; set; } = [];

    public PaginatedResult()
    {
    }

    [JsonConstructor]
    public PaginatedResult(int pageIndex, int pageSize, long totalCount, IEnumerable<TEntity> data)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
        Data = data;
    }
}