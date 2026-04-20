using GropMng.Web.Areas.Admin.Models.Owner;

namespace GropMng.Web.Areas.Admin.Factories.User;

/// <summary>
/// Prepares owner-account models for the admin back-office.
/// </summary>
public interface IOwnerModelFactory
{
    Task<OwnerSearchModel> PrepareSearchModelAsync(OwnerSearchModel? searchModel = null, CancellationToken cancellationToken = default);

    Task<OwnerListModel> PrepareListModelAsync(OwnerSearchModel searchModel, CancellationToken cancellationToken = default);

    Task<OwnerEditModel?> PrepareEditModelAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<bool> SaveEditAsync(OwnerEditModel model, CancellationToken cancellationToken = default);
}
