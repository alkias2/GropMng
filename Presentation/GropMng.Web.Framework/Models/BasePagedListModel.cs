namespace GropMng.Web.Framework.Models;

/// <summary>
/// Base response model for DataTables server-side paging.
/// Derive a concrete list model from this class and set
/// <typeparamref name="TRow"/> to the per-row view model type.
/// </summary>
/// <typeparam name="TRow">The type of a single data row returned to DataTables.</typeparam>
public abstract class BasePagedListModel<TRow>
{
    /// <summary>
    /// Echoed back from the request's <c>draw</c> parameter.
    /// DataTables uses this to match responses to the correct request.
    /// </summary>
    public string? Draw { get; set; }

    /// <summary>Total records in the unfiltered data source.</summary>
    public int RecordsTotal { get; set; }

    /// <summary>Total records after applying search and filter criteria.</summary>
    public int RecordsFiltered { get; set; }

    /// <summary>The current page of row data serialised to the DataTables <c>data</c> array.</summary>
    public IEnumerable<TRow> Data { get; set; } = Enumerable.Empty<TRow>();
}
