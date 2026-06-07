using GropMng.Web.Framework.Models;

namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// DataTables response model for the Language grid.
/// Inherits <see cref="BasePagedListModel{TRow}"/> which provides
/// the <c>draw</c>, <c>recordsTotal</c>, <c>recordsFiltered</c> and <c>data</c> fields
/// expected by the DataTables server-side protocol.
/// </summary>
public class LanguageListModel : BasePagedListModel<LanguageRowModel>
{
}
