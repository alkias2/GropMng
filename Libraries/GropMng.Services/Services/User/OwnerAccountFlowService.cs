using System.Text;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace GropMng.Services.Services.User;

/// <summary>
/// Coordinates registration, email confirmation, and password recovery for owner accounts.
/// </summary>
public class OwnerAccountFlowService : IOwnerAccountFlowService
{
    private const string RegisteredOwnerRoleSystemName = "RegisteredOwner";
    private static readonly TimeSpan EmailConfirmationTokenLifetime = TimeSpan.FromDays(2);

    private readonly IRepository<Owner> _ownerRepository;
    private readonly IRepository<OwnerRole> _ownerRoleRepository;
    private readonly IRepository<OwnerPassword> _ownerPasswordRepository;
    private readonly ISettingService _settingService;
    private readonly IOwnerPasswordService _ownerPasswordService;
    private readonly IDataProtector _emailTokenProtector;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerAccountFlowService"/> class.
    /// </summary>
    public OwnerAccountFlowService(
        IRepository<Owner> ownerRepository,
        IRepository<OwnerRole> ownerRoleRepository,
        IRepository<OwnerPassword> ownerPasswordRepository,
        ISettingService settingService,
        IOwnerPasswordService ownerPasswordService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _ownerRoleRepository = ownerRoleRepository ?? throw new ArgumentNullException(nameof(ownerRoleRepository));
        _ownerPasswordRepository = ownerPasswordRepository ?? throw new ArgumentNullException(nameof(ownerPasswordRepository));
        _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        _ownerPasswordService = ownerPasswordService ?? throw new ArgumentNullException(nameof(ownerPasswordService));
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        _emailTokenProtector = dataProtectionProvider.CreateProtector("GropMng.Owner.EmailConfirmation.v1");
    }

    /// <inheritdoc />
    public async Task<OwnerRegistrationResult> RegisterAsync(OwnerRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Email and password are required.");

        var normalizedEmail = request.Email.Trim();
        var existingOwner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.Email == normalizedEmail,
            cancellationToken: cancellationToken);

        if (existingOwner is not null)
            throw new DomainException("An account with this email already exists.");

        var settings = await _settingService.LoadAsync<GropOwnerRegistrationSettings>(cancellationToken);
        var registeredOwnerRole = await _ownerRoleRepository.FirstOrDefaultAsync(
            entity => entity.SystemName == RegisteredOwnerRoleSystemName && entity.IsActive,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (registeredOwnerRole is null)
            throw new DomainException("The RegisteredOwner role is not configured.");

        var passwordHash = _ownerPasswordService.HashPassword(request.Password);
        var now = DateTime.UtcNow;
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? string.Join(' ', new[] { request.FirstName?.Trim(), request.LastName?.Trim() }.Where(value => !string.IsNullOrWhiteSpace(value)))
            : request.DisplayName.Trim();

        var owner = new Owner
        {
            OwnerId = Guid.NewGuid(),
            FirstName = (request.FirstName ?? string.Empty).Trim(),
            LastName = (request.LastName ?? string.Empty).Trim(),
            DisplayName = displayName,
            Email = normalizedEmail,
            PasswordHash = passwordHash.Hash,
            Status = settings.RequireEmailConfirmation ? OwnerAccountStatus.PendingActivation : OwnerAccountStatus.Active,
            IsEmailConfirmed = !settings.RequireEmailConfirmation,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            IsDeleted = false,
            OwnerRoles = [registeredOwnerRole]
        };

        owner = await _ownerRepository.CreateAsync(owner, cancellationToken: cancellationToken);
        await _ownerPasswordRepository.CreateAsync(new OwnerPassword
        {
            OwnerId = owner.Id,
            PasswordHash = passwordHash.Hash,
            PasswordSalt = passwordHash.Salt,
            CreatedAtUtc = now,
            IsCurrent = true
        }, cancellationToken: cancellationToken);

        var confirmationToken = settings.RequireEmailConfirmation
            ? GenerateEmailConfirmationToken(owner)
            : null;

        return new OwnerRegistrationResult(owner, settings.RequireEmailConfirmation, confirmationToken);
    }

    /// <inheritdoc />
    public string GenerateEmailConfirmationToken(Owner owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var payload = $"{owner.OwnerId:N}|{owner.Email}|{DateTime.UtcNow.Ticks}";
        var protectedPayload = _emailTokenProtector.Protect(payload);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedPayload));
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            return false;

        var normalizedEmail = email.Trim();
        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.Email == normalizedEmail,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (owner is null)
            return false;

        if (owner.IsEmailConfirmed && owner.Status == OwnerAccountStatus.Active)
            return true;

        if (!TryReadEmailConfirmationToken(token, out var ownerId, out var protectedEmail, out var issuedAtUtc))
            return false;

        if (ownerId != owner.OwnerId || !string.Equals(protectedEmail, owner.Email, StringComparison.OrdinalIgnoreCase))
            return false;

        if (DateTime.UtcNow - issuedAtUtc > EmailConfirmationTokenLifetime)
            return false;

        owner.IsEmailConfirmed = true;
        owner.Status = OwnerAccountStatus.Active;
        owner.UpdatedAtUtc = DateTime.UtcNow;
        await _ownerRepository.UpdateAsync(owner, cancellationToken: cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<PasswordResetRequestResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new PasswordResetRequestResult(false, null, null);

        var normalizedEmail = email.Trim();
        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.Email == normalizedEmail && entity.IsActive,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (owner is null)
            return new PasswordResetRequestResult(false, null, null);

        var settings = await _settingService.LoadAsync<GropOwnerRegistrationSettings>(cancellationToken);
        var currentPassword = await GetCurrentPasswordRecordAsync(owner, cancellationToken);

        currentPassword.PasswordResetToken = _ownerPasswordService.GenerateResetToken();
        currentPassword.PasswordResetTokenExpiresAtUtc = _ownerPasswordService.CalculateResetTokenExpirationUtc(settings.PasswordResetTokenExpirationHours);

        if (currentPassword.Id > 0)
            await _ownerPasswordRepository.UpdateAsync(currentPassword, cancellationToken: cancellationToken);
        else
            await _ownerPasswordRepository.CreateAsync(currentPassword, cancellationToken: cancellationToken);

        return new PasswordResetRequestResult(true, currentPassword.PasswordResetToken, currentPassword.PasswordResetTokenExpiresAtUtc);
    }

    /// <inheritdoc />
    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        var normalizedEmail = email.Trim();
        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.Email == normalizedEmail && entity.IsActive,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (owner is null)
            return false;

        var currentPassword = await _ownerPasswordRepository.FirstOrDefaultAsync(
            entity => entity.OwnerId == owner.Id && entity.IsCurrent,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (currentPassword is null
            || !string.Equals(currentPassword.PasswordResetToken, token, StringComparison.Ordinal)
            || currentPassword.PasswordResetTokenExpiresAtUtc is null
            || currentPassword.PasswordResetTokenExpiresAtUtc.Value < DateTime.UtcNow)
        {
            return false;
        }

        currentPassword.IsCurrent = false;
        currentPassword.PasswordResetToken = null;
        currentPassword.PasswordResetTokenExpiresAtUtc = null;
        await _ownerPasswordRepository.UpdateAsync(currentPassword, cancellationToken: cancellationToken);

        var passwordHash = _ownerPasswordService.HashPassword(newPassword);
        owner.PasswordHash = passwordHash.Hash;
        owner.UpdatedAtUtc = DateTime.UtcNow;
        await _ownerRepository.UpdateAsync(owner, cancellationToken: cancellationToken);

        await _ownerPasswordRepository.CreateAsync(new OwnerPassword
        {
            OwnerId = owner.Id,
            PasswordHash = passwordHash.Hash,
            PasswordSalt = passwordHash.Salt,
            CreatedAtUtc = DateTime.UtcNow,
            IsCurrent = true
        }, cancellationToken: cancellationToken);

        return true;
    }

    private async Task<OwnerPassword> GetCurrentPasswordRecordAsync(Owner owner, CancellationToken cancellationToken)
    {
        var currentPassword = await _ownerPasswordRepository.FirstOrDefaultAsync(
            entity => entity.OwnerId == owner.Id && entity.IsCurrent,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (currentPassword is not null)
            return currentPassword;

        return new OwnerPassword
        {
            OwnerId = owner.Id,
            PasswordHash = owner.PasswordHash,
            PasswordSalt = string.Empty,
            CreatedAtUtc = DateTime.UtcNow,
            IsCurrent = true
        };
    }

    private bool TryReadEmailConfirmationToken(string token, out Guid ownerId, out string email, out DateTime issuedAtUtc)
    {
        ownerId = Guid.Empty;
        email = string.Empty;
        issuedAtUtc = DateTime.MinValue;

        try
        {
            var protectedPayload = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var payload = _emailTokenProtector.Unprotect(protectedPayload);
            var parts = payload.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3 || !Guid.TryParse(parts[0], out ownerId) || !long.TryParse(parts[2], out var ticks))
                return false;

            email = parts[1];
            issuedAtUtc = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
