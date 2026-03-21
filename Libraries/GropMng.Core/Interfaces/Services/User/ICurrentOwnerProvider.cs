namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Resolves the current owner context for operations that require owner scoping.
/// </summary>
public interface ICurrentOwnerProvider
{
    /// <summary>
    /// Gets the current owner business identifier.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The owner business identifier.</returns>
    Task<Guid> GetCurrentOwnerIdAsync(CancellationToken cancellationToken = default);
}
