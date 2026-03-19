namespace GropMng.Web.Framework.Models;

/// <summary>
/// Represents the contract for models that carry DataTables server-side paging parameters.
/// Implemented by <see cref="BaseSearchModel"/> to provide consistent Page/PageSize access
/// regardless of the raw DataTables start/length offset arithmetic.
/// </summary>
public interface IPagingRequestModel
{
    /// <summary>Gets the 1-based page number derived from DataTables start/length parameters.</summary>
    int Page { get; }

    /// <summary>Gets the number of rows per page.</summary>
    int PageSize { get; }
}
