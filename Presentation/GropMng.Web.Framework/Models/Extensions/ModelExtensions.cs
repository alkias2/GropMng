using GropMng.Core;

namespace GropMng.Web.Framework.Models.Extensions;

/// <summary>
/// Extension methods for populating <see cref="BasePagedListModel{TRow}"/> instances
/// from paged query results returned by application repositories.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Fills <paramref name="listModel"/> with paging metadata and row data derived from
    /// a paged repository result, then returns the same instance for a fluent call.
    /// </summary>
    /// <typeparam name="TListModel">Concrete list model type.</typeparam>
    /// <typeparam name="TRow">Per-row view model type exposed to DataTables.</typeparam>
    /// <typeparam name="TObject">Domain entity type returned by the repository.</typeparam>
    /// <param name="listModel">The list model to populate.</param>
    /// <param name="searchModel">The search model carrying the <c>draw</c> counter.</param>
    /// <param name="objectList">Paged domain objects from the repository.</param>
    /// <param name="dataFillFunction">
    /// Projection that maps <typeparamref name="TObject"/> items to <typeparamref name="TRow"/> rows.
    /// </param>
    /// <returns>The populated <paramref name="listModel"/>.</returns>
    public static TListModel PrepareToGrid<TListModel, TRow, TObject>(
        this TListModel listModel,
        BaseSearchModel searchModel,
        IPagedList<TObject> objectList,
        Func<IEnumerable<TRow>> dataFillFunction)
        where TListModel : BasePagedListModel<TRow>
    {
        ArgumentNullException.ThrowIfNull(listModel);
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(objectList);
        ArgumentNullException.ThrowIfNull(dataFillFunction);

        listModel.Data = dataFillFunction();
        listModel.Draw = searchModel.Draw;
        listModel.RecordsTotal = objectList.TotalCount;
        listModel.RecordsFiltered = objectList.TotalCount;
        return listModel;
    }
}
