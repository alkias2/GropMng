namespace GropMng.Core.Domain.Garden.Owners;

/// <summary>
/// Stores password history and reset token metadata for an owner.
/// </summary>
public class OwnerPassword : BaseEntity
{
    public int OwnerId { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsCurrent { get; set; } = true;

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiresAtUtc { get; set; }

    public Owner Owner { get; set; } = null!;
}
