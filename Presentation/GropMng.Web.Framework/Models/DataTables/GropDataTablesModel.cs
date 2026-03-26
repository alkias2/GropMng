namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Main DataTables grid configuration model.
/// Declaratively defines the table structure, filters, columns, and actions.
/// </summary>
public class GropDataTablesModel
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropDataTablesModel"/> class.
    /// </summary>
    public GropDataTablesModel()
    {
        ColumnCollection = new List<GropColumnProperty>();
        Filters = new List<GropFilterParameter>();
        LengthMenu = new[] { 10, 25, 50, 100 };
        Length = 10;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the unique name identifier for this table.
    /// Used as the HTML table ID and for JavaScript references.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the AJAX data URL configuration.
    /// Points to the controller action that returns the list data as JSON.
    /// </summary>
    public GropDataUrl UrlRead { get; set; }

    /// <summary>
    /// Gets or sets the HTML element ID of the search/apply filters button.
    /// </summary>
    public string SearchButtonId { get; set; }

    /// <summary>
    /// Gets or sets the initial page length (number of records per page).
    /// Default: 10
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the available page length options for users to select from.
    /// Default: [10, 25, 50, 100]
    /// </summary>
    public int[] LengthMenu { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the length menu dropdown.
    /// Default: true
    /// </summary>
    public bool ShowLengthMenu { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the Info text (e.g., "Showing 1 to 10 of 100 entries").
    /// Default: true
    /// </summary>
    public bool ShowInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the Pagination controls.
    /// Default: true
    /// </summary>
    public bool ShowPagination { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of filter parameters for the grid.
    /// </summary>
    public List<GropFilterParameter> Filters { get; set; }

    /// <summary>
    /// Gets or sets the collection of column property definitions.
    /// </summary>
    public List<GropColumnProperty> ColumnCollection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether server-side processing is enabled.
    /// Default: true
    /// </summary>
    public bool ServerSideProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the search input field.
    /// Default: true
    /// </summary>
    public bool ShowSearch { get; set; } = true;

    #endregion
}
