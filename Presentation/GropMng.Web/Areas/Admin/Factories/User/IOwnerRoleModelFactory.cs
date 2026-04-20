using GropMng.Web.Areas.Admin.Models.Roles;

namespace GropMng.Web.Areas.Admin.Factories.User;

/// <summary>
/// Prepares role and permission models for the admin back-office.
/// </summary>
public interface IOwnerRoleModelFactory
{
    Task<OwnerRoleSearchModel> PrepareSearchModelAsync(OwnerRoleSearchModel? searchModel = null, CancellationToken cancellationToken = default);

    Task<OwnerRoleListModel> PrepareListModelAsync(OwnerRoleSearchModel searchModel, CancellationToken cancellationToken = default);

    Task<OwnerRoleEditModel?> PrepareEditModelAsync(int roleId, CancellationToken cancellationToken = default);

    Task<bool> SaveEditAsync(OwnerRoleEditModel model, CancellationToken cancellationToken = default);
}
