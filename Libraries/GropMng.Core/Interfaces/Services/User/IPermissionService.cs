using GropMng.Core.Domain.Garden.Owners;

namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Evaluates whether owners can perform permission-protected actions.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Authorizes the current owner against a permission system name.
    /// </summary>
    Task<bool> AuthorizeAsync(string permissionSystemName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authorizes a specific owner against a permission system name.
    /// </summary>
    Task<bool> AuthorizeAsync(string permissionSystemName, Owner owner, CancellationToken cancellationToken = default);
}
