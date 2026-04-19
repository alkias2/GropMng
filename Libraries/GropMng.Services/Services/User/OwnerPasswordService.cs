using System.Security.Cryptography;
using System.Text;
using GropMng.Core.Interfaces.Services.User;

namespace GropMng.Services.Services.User;

/// <summary>
/// Provides secure password hashing and verification helpers for owner accounts.
/// </summary>
public class OwnerPasswordService : IOwnerPasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int IterationCount = 100_000;

    /// <inheritdoc />
    public PasswordHashResult HashPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(rawPassword));

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(rawPassword, saltBytes, IterationCount, HashAlgorithmName.SHA512, HashSize);

        return new PasswordHashResult(
            Convert.ToHexString(hashBytes),
            Convert.ToHexString(saltBytes));
    }

    /// <inheritdoc />
    public bool VerifyPassword(string rawPassword, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(rawPassword) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        if (passwordHash.Contains('$', StringComparison.Ordinal))
        {
            var parts = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4 && int.TryParse(parts[1], out var iterations))
            {
                return VerifyPbkdf2(rawPassword, parts[3], parts[2], iterations);
            }
        }

        if (string.IsNullOrWhiteSpace(passwordSalt))
        {
            var legacyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawPassword)));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(legacyHash),
                Encoding.UTF8.GetBytes(passwordHash));
        }

        return VerifyPbkdf2(rawPassword, passwordHash, passwordSalt, IterationCount);
    }

    /// <inheritdoc />
    public string GenerateResetToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    }

    /// <inheritdoc />
    public DateTime CalculateResetTokenExpirationUtc(int expirationHours, DateTime? utcNow = null)
    {
        var effectiveHours = expirationHours <= 0 ? 24 : expirationHours;
        return (utcNow ?? DateTime.UtcNow).AddHours(effectiveHours);
    }

    private static bool VerifyPbkdf2(string rawPassword, string storedHash, string storedSalt, int iterations)
    {
        if (string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
            return false;

        var saltBytes = Convert.FromHexString(storedSalt);
        var actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(rawPassword, saltBytes, iterations, HashAlgorithmName.SHA512, HashSize);
        var expectedHashBytes = Convert.FromHexString(storedHash);

        return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }
}
