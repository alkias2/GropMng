using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Framework.Models.DataTables;
using Microsoft.AspNetCore.Routing;

namespace GropMng.Web.Factories.Plant;

/// <summary>
/// Provides declarative DataTables configuration for the Plant admin grid.
/// </summary>
public static class PlantGridDefinition
{
    #region Public

    /// <summary>
    /// Builds the <see cref="GropDataTablesModel"/> for the Plant index grid.
    /// </summary>
    /// <param name="searchModel">The incoming search model that provides paging defaults.</param>
    /// <param name="titles">The localized table header titles.</param>
    /// <returns>A fully configured <see cref="GropDataTablesModel"/> instance.</returns>
    public static GropDataTablesModel Build(PlantSearchModel searchModel, PlantGridColumnTitles titles)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(titles);

        return new GropDataTablesModel
        {
            Name = "plantsTable",
            UrlRead = new GropDataUrl("List", "Plant", new RouteValueDictionary(new { area = "Admin" })),
            Length = searchModel.PageSize,
            LengthMenu = [10, 25, 50, 100],
            ShowSearch = false,
            Filters =
            [
                new GropFilterParameter("searchTerm")
                {
                    ElementId = "searchTermFilter",
                },
                new GropFilterParameter("category")
                {
                    ElementId = "categoryFilter",
                },
            ],
            ColumnCollection =
            [
                new GropColumnProperty("id")
                {
                    Title = titles.Id,
                    Width = "60px",
                },
                new GropColumnProperty("commonName")
                {
                    Title = titles.CommonName,
                },
                new GropColumnProperty("scientificName")
                {
                    Title = titles.ScientificName,
                },
                new GropColumnProperty("family")
                {
                    Title = titles.Family,
                    Render = new RenderCustom("window.GropPlantGridRenderers.renderFamily"),
                },
                new GropColumnProperty("category")
                {
                    Title = titles.Category,
                    Render = new RenderCustom("window.GropPlantGridRenderers.renderCategory"),
                },
                new GropColumnProperty("id")
                {
                    Title = titles.Flags,
                    Orderable = false,
                    Searchable = false,
                    Render = new RenderCustom("window.GropPlantGridRenderers.renderFlags"),
                },
                new GropColumnProperty("id")
                {
                    Title = titles.Actions,
                    Width = "180px",
                    ClassName = "text-center",
                    Orderable = false,
                    Searchable = false,
                    Render = new RenderCustom("window.GropPlantGridRenderers.renderActions"),
                },
            ],
        };
    }

    #endregion
}

/// <summary>
/// Represents localized table header titles for the Plant grid.
/// </summary>
public sealed class PlantGridColumnTitles
{
    /// <summary>
    /// Gets or sets the title for the Id column.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Common Name column.
    /// </summary>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Scientific Name column.
    /// </summary>
    public string ScientificName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Family column.
    /// </summary>
    public string Family { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Category column.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Flags column.
    /// </summary>
    public string Flags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Actions column.
    /// </summary>
    public string Actions { get; set; } = string.Empty;
}