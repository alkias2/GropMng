using GropMng.Core.Domain.Garden.Owners;

namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Manages the custom cookie-based authentication flow for owners.
/// </summary>
public interface IOwnerAuthenticationService
{
    /// <summary>
    /// Signs in the supplied owner using the application's cookie authentication scheme.
    /// </summary>
    Task SignInAsync(Owner owner, bool isPersistent = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates owner credentials and returns the authenticated owner when they are correct.
    /// </summary>
    Task<Owner?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs out the current owner from the application.
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Resolves the authenticated owner from the current HTTP context.
    /// </summary>
    Task<Owner?> GetAuthenticatedOwnerAsync(CancellationToken cancellationToken = default);
}
