using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.Roles;

/// <summary>
/// Search parameters for the owner-role administration grid.
/// </summary>
public class OwnerRoleSearchModel : BaseSearchModel
{
    public GropDataTablesModel GridModel { get; set; } = new();

    public string? SearchTerm { get; set; }
}
