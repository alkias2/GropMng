using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Factories.Plant;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Tests.Factories;

/// <summary>
/// Verifies declarative DataTables definitions for admin grids.
/// </summary>
public class GridDefinitionTests
{
    /// <summary>
    /// Ensures Plant grid configuration has expected filters, columns and route metadata.
    /// </summary>
    [Fact]
    public void PlantGridDefinition_Build_ReturnsExpectedStructure()
    {
        var searchModel = new PlantSearchModel();
        var titles = new PlantGridColumnTitles
        {
            Id = "ID",
            CommonName = "Common Name",
            ScientificName = "Scientific Name",
            Family = "Family",
            Category = "Category",
            Flags = "Flags",
            Actions = "Actions"
        };

        var model = PlantGridDefinition.Build(searchModel, titles);

        Assert.Equal("plantsTable", model.Name);
        Assert.Equal("List", model.UrlRead.ActionName);
        Assert.Equal("Plant", model.UrlRead.ControllerName);
        Assert.Equal(searchModel.PageSize, model.Length);
        Assert.False(model.ShowSearch);
        Assert.Equal(2, model.Filters.Count);
        Assert.Equal(7, model.ColumnCollection.Count);
        Assert.Equal("searchTerm", model.Filters[0].Name);
        Assert.Equal("category", model.Filters[1].Name);
    }

    /// <summary>
    /// Ensures Plant custom render mappings point to the expected JavaScript renderer functions.
    /// </summary>
    [Fact]
    public void PlantGridDefinition_Build_ConfiguresExpectedCustomRenderers()
    {
        var model = PlantGridDefinition.Build(new PlantSearchModel(), new PlantGridColumnTitles());

        Assert.Equal("window.GropPlantGridRenderers.renderFamily", ((RenderCustom)model.ColumnCollection[3].Render).FunctionName);
        Assert.Equal("window.GropPlantGridRenderers.renderCategory", ((RenderCustom)model.ColumnCollection[4].Render).FunctionName);
        Assert.Equal("window.GropPlantGridRenderers.renderFlags", ((RenderCustom)model.ColumnCollection[5].Render).FunctionName);
        Assert.Equal("window.GropPlantGridRenderers.renderActions", ((RenderCustom)model.ColumnCollection[6].Render).FunctionName);
    }
}
