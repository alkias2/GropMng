namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Provides password hashing, verification, and reset-token helpers for owner accounts.
/// </summary>
public interface IOwnerPasswordService
{
    /// <summary>
    /// Hashes a raw password and returns the generated hash and salt.
    /// </summary>
    PasswordHashResult HashPassword(string rawPassword);

    /// <summary>
    /// Verifies a raw password against the stored hash and salt.
    /// </summary>
    bool VerifyPassword(string rawPassword, string passwordHash, string passwordSalt);

    /// <summary>
    /// Generates a secure password reset token.
    /// </summary>
    string GenerateResetToken();

    /// <summary>
    /// Calculates the UTC expiration timestamp for a password reset token.
    /// </summary>
    DateTime CalculateResetTokenExpirationUtc(int expirationHours, DateTime? utcNow = null);
}

/// <summary>
/// Represents the output of a password hashing operation.
/// </summary>
/// <param name="Hash">The generated password hash.</param>
/// <param name="Salt">The generated password salt.</param>
public readonly record struct PasswordHashResult(string Hash, string Salt);
