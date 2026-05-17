using GropMng.Core.Domain.Garden.Owners;

namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Coordinates owner registration, email confirmation, and password recovery flows.
/// </summary>
public interface IOwnerAccountFlowService
{
    /// <summary>
    /// Registers a new owner account according to the current registration settings.
    /// </summary>
    Task<OwnerRegistrationResult> RegisterAsync(OwnerRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an email confirmation token for the supplied owner.
    /// </summary>
    string GenerateEmailConfirmationToken(Owner owner);

    /// <summary>
    /// Confirms an owner's email address using the supplied token.
    /// </summary>
    Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the password reset flow for the supplied email address.
    /// </summary>
    Task<PasswordResetRequestResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the owner's password when the token is valid and not expired.
    /// </summary>
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes an owner's password from the admin management flow.
    /// </summary>
    Task<ChangeOwnerPasswordResult> ChangeOwnerPasswordAsync(ChangeOwnerPasswordRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the required data for owner self-registration.
/// </summary>
public sealed record OwnerRegistrationRequest(
    string FirstName,
    string LastName,
    string DisplayName,
    string Email,
    string Password);

/// <summary>
/// Represents the outcome of a registration operation.
/// </summary>
public sealed record OwnerRegistrationResult(
    Owner Owner,
    bool RequiresEmailConfirmation,
    string? EmailConfirmationToken);

/// <summary>
/// Represents the outcome of a password reset request.
/// </summary>
public sealed record PasswordResetRequestResult(
    bool EmailMatched,
    string? ResetToken,
    DateTime? ExpiresAtUtc);

/// <summary>
/// Represents the input required to change an owner's password from admin management.
/// </summary>
public sealed record ChangeOwnerPasswordRequest(
    Guid OwnerId,
    string NewPassword);

/// <summary>
/// Represents the outcome of an owner password change operation.
/// </summary>
public sealed class ChangeOwnerPasswordResult
{
    private readonly List<string> _errors = [];

    public bool Success => _errors.Count == 0;

    public IReadOnlyList<string> Errors => _errors;

    public void AddError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            return;

        _errors.Add(error.Trim());
    }
}
