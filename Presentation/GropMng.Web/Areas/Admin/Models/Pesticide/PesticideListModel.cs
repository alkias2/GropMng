using GropMng.Web.Framework.Models;

namespace GropMng.Web.Areas.Admin.Models.Pesticide;

/// <summary>
/// DataTables response model wrapping a paged list of pesticide rows.
/// </summary>
public class PesticideListModel : BasePagedListModel<PesticideRowModel>
{
}
