using GropMng.Web.Framework.Models;

namespace GropMng.Web.Areas.Admin.Models.Fertilizer;

/// <summary>
/// DataTables response model wrapping a paged list of fertilizer rows.
/// </summary>
public class FertilizerListModel : BasePagedListModel<FertilizerRowModel>
{
}
