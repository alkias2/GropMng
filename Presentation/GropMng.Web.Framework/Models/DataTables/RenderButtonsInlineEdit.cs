namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying inline "Edit" and "Delete" action buttons for edit mode.
/// </summary>
public class RenderButtonsInlineEdit : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderButtonsInlineEdit"/> class.
    /// </summary>
    /// <param name="editUrl">The save endpoint for inline edits.</param>
    /// <param name="deleteUrl">The delete endpoint for inline deletes.</param>
    public RenderButtonsInlineEdit(GropDataUrl editUrl, GropDataUrl deleteUrl)
    {
        EditUrl = editUrl;
        DeleteUrl = deleteUrl;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "buttons-inline-edit";

    /// <summary>
    /// Gets or sets the save endpoint for inline edits.
    /// </summary>
    public GropDataUrl EditUrl { get; set; }

    /// <summary>
    /// Gets or sets the delete endpoint for inline deletes.
    /// </summary>
    public GropDataUrl DeleteUrl { get; set; }

    #endregion
}
