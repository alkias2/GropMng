using GropMng.Core.Configuration;

namespace GropMng.Core.Domain.Configuration;

/// <summary>
/// Registration and password recovery settings for owner accounts.
/// </summary>
public class GropOwnerRegistrationSettings : ISettings
{
    public bool RequireEmailConfirmation { get; set; } = false;

    public int PasswordResetTokenExpirationHours { get; set; } = 24;
}
