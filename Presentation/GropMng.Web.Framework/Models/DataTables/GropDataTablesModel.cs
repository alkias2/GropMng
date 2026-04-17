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
        Processing = true;
        ServerSide = true;
        Paging = true;
        Info = true;
        Ordering = true;
        RefreshButton = true;
        PagingType = "simple_numbers";
        Length = 10;
        LengthMenu = new[] { 10, 25, 50, 100 };
        ShowLengthMenu = true;
        ShowSearch = true;
        Filters = new List<GropFilterParameter>();
        ClearFilterIds = new List<string>();
        ColumnCollection = new List<GropColumnProperty>();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the unique name identifier for this table.
    /// Used as the HTML table ID and for JavaScript references.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the AJAX data URL configuration for reading (listing) data.
    /// Points to the controller action that returns the list data as JSON.
    /// </summary>
    public GropDataUrl UrlRead { get; set; }

    /// <summary>
    /// Gets or sets the AJAX data URL configuration for delete operations.
    /// When set, <see cref="Table.cshtml"/> auto-generates the per-table delete JS function.
    /// </summary>
    public GropDataUrl UrlDelete { get; set; }

    /// <summary>
    /// Gets or sets the AJAX data URL configuration for inline update operations.
    /// When set, <see cref="Table.cshtml"/> auto-generates the per-table update JS function.
    /// </summary>
    public GropDataUrl UrlUpdate { get; set; }

    /// <summary>
    /// Gets or sets the HTML element ID of the search/apply filters button.
    /// </summary>
    public string SearchButtonId { get; set; }

    /// <summary>
    /// Gets or sets the HTML element ID of the clear-filters button.
    /// When provided, the shared table script clears all configured filter inputs.
    /// </summary>
    public string ClearButtonId { get; set; }

    /// <summary>
    /// Gets or sets the collection of HTML element IDs that should be reset
    /// when the clear-filters button is clicked.
    /// Optional: when omitted, the shared table helper derives the IDs automatically
    /// from the configured <see cref="Filters" /> collection.
    /// </summary>
    public IList<string> ClearFilterIds { get; set; }

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
    /// Gets or sets the length menu as a JavaScript array literal string for DataTables.
    /// Derived automatically from <see cref="LengthMenu"/>.
    /// Example: "[10, 25, 50, 100]"
    /// </summary>
    public string LengthMenuJs =>
        LengthMenu is { Length: > 0 }
            ? "[" + string.Join(", ", LengthMenu) + "]"
            : "[10, 25, 50, 100]";

    /// <summary>
    /// Gets or sets a value indicating whether to show the length menu dropdown.
    /// Default: true. GropMng extension.
    /// </summary>
    public bool ShowLengthMenu { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the info text is shown
    /// (e.g., "Showing 1 to 10 of 100 entries").
    /// Default: true.
    /// </summary>
    public bool Info { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether pagination controls are shown.
    /// Default: true.
    /// </summary>
    public bool Paging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the DataTables processing indicator is shown.
    /// Default: true.
    /// </summary>
    public bool Processing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether server-side processing is enabled.
    /// Default: true.
    /// </summary>
    public bool ServerSide { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether column ordering (sorting) is enabled.
    /// Default: true.
    /// </summary>
    public bool Ordering { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show the built-in DataTables search input.
    /// Default: true. GropMng extension.
    /// </summary>
    public bool ShowSearch { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show a refresh button.
    /// Default: true.
    /// </summary>
    public bool RefreshButton { get; set; } = true;

    /// <summary>
    /// Gets or sets the pagination button display style.
    /// Default: <c>"simple_numbers"</c>.
    /// See https://datatables.net/reference/option/pagingType
    /// </summary>
    public string PagingType { get; set; }

    /// <summary>
    /// Gets or sets the DOM positioning string for DataTables features.
    /// See https://datatables.net/reference/option/dom
    /// </summary>
    public string Dom { get; set; }

    /// <summary>
    /// Gets or sets the name of the JavaScript function called after each draw.
    /// See https://datatables.net/reference/option/drawCallback
    /// </summary>
    public string DrawCallback { get; set; }

    /// <summary>
    /// Gets or sets the name of the JavaScript function called when the header is drawn.
    /// See https://datatables.net/reference/option/headerCallback
    /// </summary>
    public string HeaderCallback { get; set; }

    /// <summary>
    /// Gets or sets the number of columns to generate in the table footer.
    /// Set to 0 (default) to disable the footer.
    /// </summary>
    public int FooterColumns { get; set; }

    /// <summary>
    /// Gets or sets the name of the JavaScript function called when the footer is drawn.
    /// See https://datatables.net/reference/option/footerCallback
    /// </summary>
    public string FooterCallback { get; set; }

    /// <summary>
    /// Gets or sets the row data field name used to set the HTML <c>id</c> attribute on each row.
    /// </summary>
    public string RowIdBasedOnField { get; set; }

    /// <summary>
    /// Gets or sets the column data field name used in delete actions.
    /// When not set, <c>"id"</c> is used as the default delete parameter name.
    /// </summary>
    public string BindColumnNameActionDelete { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this table is a child (nested) DataTable.
    /// </summary>
    public bool IsChildTable { get; set; }

    /// <summary>
    /// Gets or sets the child DataTable model for master-detail / nested grid scenarios.
    /// When set, <see cref="Table.cshtml"/> auto-generates the child table expand/collapse JS.
    /// </summary>
    public GropDataTablesModel ChildTable { get; set; }

    /// <summary>
    /// Gets or sets the parent table primary key column name.
    /// Used to pass the parent row ID to the child table query.
    /// </summary>
    public string PrimaryKeyColumn { get; set; }

    /// <summary>
    /// Gets or sets static data for the table (JS array or object).
    /// Used instead of <see cref="UrlRead"/> for client-side only tables.
    /// </summary>
    public object Data { get; set; }

    /// <summary>
    /// Gets or sets the list of filter parameters for the grid.
    /// </summary>
    public List<GropFilterParameter> Filters { get; set; }

    /// <summary>
    /// Gets or sets the collection of column property definitions.
    /// </summary>
    public List<GropColumnProperty> ColumnCollection { get; set; }

    #endregion
}
