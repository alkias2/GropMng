namespace GropMng.Core;

/// <summary>
/// Represents a concrete paged list implementation backed by an in-memory list.
/// Provides paging metadata together with the current page item collection.
/// </summary>
[Serializable]
public class PagedList<T> : List<T>, IPagedList<T>
{
	/// <summary>
	/// Initializes a new paged list from a queryable source.
	/// </summary>
	/// <param name="source">The source query to page.</param>
	/// <param name="pageIndex">Zero-based page index.</param>
	/// <param name="pageSize">Number of items per page.</param>
	/// <param name="getOnlyTotalCount">When true, only paging metadata is computed.</param>
	public PagedList(IQueryable<T> source, int pageIndex, int pageSize, bool getOnlyTotalCount = false)
	{
		var total = source.Count();
		TotalCount = total;
		TotalPages = total / pageSize;
		if (total % pageSize > 0)
			TotalPages++;

		PageSize = pageSize;
		PageIndex = pageIndex;

		if (getOnlyTotalCount)
			return;

		AddRange(source.Skip(pageIndex * pageSize).Take(pageSize).ToList());
	}

	/// <summary>
	/// Initializes a new paged list from an in-memory list source.
	/// </summary>
	/// <param name="source">The source list to page.</param>
	/// <param name="pageIndex">Zero-based page index.</param>
	/// <param name="pageSize">Number of items per page.</param>
	public PagedList(IList<T> source, int pageIndex, int pageSize)
	{
		TotalCount = source.Count;
		TotalPages = TotalCount / pageSize;
		if (TotalCount % pageSize > 0)
			TotalPages++;

		PageSize = pageSize;
		PageIndex = pageIndex;

		AddRange(source.Skip(pageIndex * pageSize).Take(pageSize));
	}

	/// <summary>
	/// Initializes a new paged list from a pre-paged source and known total count.
	/// </summary>
	/// <param name="source">The already paged item subset.</param>
	/// <param name="pageIndex">Zero-based page index.</param>
	/// <param name="pageSize">Number of items per page.</param>
	/// <param name="totalCount">Total number of items across all pages.</param>
	public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
	{
		TotalCount = totalCount;
		TotalPages = TotalCount / pageSize;
		if (TotalCount % pageSize > 0)
			TotalPages++;

		PageSize = pageSize;
		PageIndex = pageIndex;

		AddRange(source);
	}

	/// <summary>
	/// Gets the zero-based current page index.
	/// </summary>
	public int PageIndex { get; }

	/// <summary>
	/// Gets the configured page size.
	/// </summary>
	public int PageSize { get; }

	/// <summary>
	/// Gets the total item count across all pages.
	/// </summary>
	public int TotalCount { get; }

	/// <summary>
	/// Gets the total number of pages.
	/// </summary>
	public int TotalPages { get; }

	/// <summary>
	/// Gets a value indicating whether a previous page exists.
	/// </summary>
	public bool HasPreviousPage => PageIndex > 0;

	/// <summary>
	/// Gets a value indicating whether a next page exists.
	/// </summary>
	public bool HasNextPage => PageIndex + 1 < TotalPages;
}
