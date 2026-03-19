namespace GropMng.Web.Framework.Models;

/// <summary>
/// Base class for all DataTables server-side search and filter models.
/// Translates the raw DataTables HTTP parameters (draw/start/length) into
/// 1-based page numbers used by the application repositories.
/// </summary>
public abstract class BaseSearchModel : IPagingRequestModel
{
    /// <summary>
    /// DataTables draw counter. Echoed back in the JSON response to prevent
    /// out-of-order replies when the user pages/filters rapidly.
    /// </summary>
    public string? Draw { get; set; }

    /// <summary>
    /// Zero-based row offset sent by DataTables (e.g. 0, 10, 20 …).
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Page length (rows per page) sent by DataTables. Defaults to 10.
    /// </summary>
    public int Length { get; set; } = 10;

    /// <inheritdoc />
    /// <remarks>Converts the zero-based DataTables offset to a 1-based page index.</remarks>
    public int Page => Length > 0 ? (Start / Length) + 1 : 1;

    /// <inheritdoc />
    public int PageSize => Length > 0 ? Length : 10;

    /// <summary>
    /// Clamps <see cref="Length"/> to a safe maximum and resets <see cref="Start"/>
    /// to zero (first page). Use when pre-populating the initial GET search model.
    /// </summary>
    /// <param name="pageSize">Desired page size. Defaults to 10.</param>
    /// <param name="maxPageSize">Hard ceiling on page size. Defaults to 200.</param>
    public virtual void SetGridPageSize(int pageSize = 10, int maxPageSize = 200)
    {
        Length = Math.Clamp(pageSize, 1, maxPageSize);
        Start = 0;
    }
}
