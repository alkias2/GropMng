using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Data.DbContext;
using GropMng.Web.Infrastructure.Security;
using GropMng.Web.Initialization.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GropMng.Web.Initialization.Seeders;

/// <summary>
/// Seeds the baseline owner, role, and registration-setting records required by the authorization subsystem.
/// </summary>
internal sealed class OwnerSeeder
{
    private static readonly Guid DefaultOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string DefaultOwnerEmail = "owner@gropmng.local";
    private const string AdministratorRoleSystemName = "Administrator";
    private const string RegisteredOwnerRoleSystemName = "RegisteredOwner";
    private const string RegistrationSettingsPrefix = "grop.gropownerregistrationsettings.";

    private readonly GropContext _dbContext;
    private readonly ISettingService _settingService;
    private readonly IOwnerPasswordService _ownerPasswordService;
    private readonly OwnerBootstrapOptions _bootstrapOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="settingService">The application setting service.</param>
    /// <param name="bootstrapOptions">The administrator bootstrap options.</param>
    public OwnerSeeder(
        GropContext dbContext,
        ISettingService settingService,
        IOwnerPasswordService ownerPasswordService,
        IOptions<OwnerBootstrapOptions> bootstrapOptions)
    {
        _dbContext = dbContext;
        _settingService = settingService;
        _ownerPasswordService = ownerPasswordService;
        _bootstrapOptions = bootstrapOptions.Value;
    }

    /// <summary>
    /// Seeds the default authorization foundation if it does not already exist.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous seed operation.</returns>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDefaultRegistrationSettingsAsync(cancellationToken);

        var administratorRole = await EnsureRoleAsync(
            name: "Administrator",
            systemName: AdministratorRoleSystemName,
            description: "Full access. Manages settings, owners, roles, and permissions.",
            cancellationToken: cancellationToken);

        var registeredOwnerRole = await EnsureRoleAsync(
            name: "Registered Owner",
            systemName: RegisteredOwnerRoleSystemName,
            description: "Standard authenticated owner who manages their own garden workspace.",
            cancellationToken: cancellationToken);

        await EnsurePermissionsAsync(administratorRole, registeredOwnerRole, cancellationToken);

        var owner = await EnsureAdministratorOwnerAsync(cancellationToken);
        await EnsureAdministratorMembershipAsync(owner, administratorRole, cancellationToken);
    }

    private async Task EnsureDefaultRegistrationSettingsAsync(CancellationToken cancellationToken)
    {
        var existing = await _settingService.GetAllByPrefixAsync(RegistrationSettingsPrefix, cancellationToken);

        if (!existing.ContainsKey(RegistrationSettingsPrefix + "requireemailconfirmation"))
        {
            await _settingService.SetByKeyAsync(
                RegistrationSettingsPrefix + "requireemailconfirmation",
                false,
                cancellationToken);
        }

        if (!existing.ContainsKey(RegistrationSettingsPrefix + "passwordresettokenexpirationhours"))
        {
            await _settingService.SetByKeyAsync(
                RegistrationSettingsPrefix + "passwordresettokenexpirationhours",
                24,
                cancellationToken);
        }
    }

    private async Task<OwnerRole> EnsureRoleAsync(
        string name,
        string systemName,
        string description,
        CancellationToken cancellationToken)
    {
        var role = await _dbContext.OwnerRoles.FirstOrDefaultAsync(
            entity => entity.SystemName == systemName,
            cancellationToken);

        if (role is not null)
            return role;

        role = new OwnerRole
        {
            Name = name,
            SystemName = systemName,
            Description = description,
            IsActive = true,
            IsSystemRole = true
        };

        _dbContext.OwnerRoles.Add(role);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return role;
    }

    private async Task<Owner> EnsureAdministratorOwnerAsync(CancellationToken cancellationToken)
    {
        var email = string.IsNullOrWhiteSpace(_bootstrapOptions.AdministratorEmail)
            ? DefaultOwnerEmail
            : _bootstrapOptions.AdministratorEmail.Trim();

        var firstName = string.IsNullOrWhiteSpace(_bootstrapOptions.AdministratorFirstName)
            ? "System"
            : _bootstrapOptions.AdministratorFirstName.Trim();

        var lastName = string.IsNullOrWhiteSpace(_bootstrapOptions.AdministratorLastName)
            ? "Administrator"
            : _bootstrapOptions.AdministratorLastName.Trim();

        var displayName = string.IsNullOrWhiteSpace(_bootstrapOptions.AdministratorDisplayName)
            ? string.Join(' ', new[] { firstName, lastName }.Where(value => !string.IsNullOrWhiteSpace(value)))
            : _bootstrapOptions.AdministratorDisplayName.Trim();

        var password = string.IsNullOrWhiteSpace(_bootstrapOptions.AdministratorPassword)
            ? "ChangeMe123!"
            : _bootstrapOptions.AdministratorPassword;

        // Look up by business identifier first to avoid duplicate key violations (UQ_Owner_OwnerId).
        // The email may have changed across bootstrap configuration updates, but the business ID is stable.
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(entity => entity.OwnerId == DefaultOwnerBusinessId, cancellationToken)
                   ?? await _dbContext.Owners.FirstOrDefaultAsync(entity => entity.Email == email, cancellationToken);

        if (owner is null)
        {
            var now = DateTime.UtcNow;
            var passwordHashResult = _ownerPasswordService.HashPassword(password);

            owner = new Owner
            {
                OwnerId = DefaultOwnerBusinessId,
                FirstName = firstName,
                LastName = lastName,
                DisplayName = displayName,
                Email = email,
                PasswordHash = passwordHashResult.Hash,
                Status = OwnerAccountStatus.Active,
                IsEmailConfirmed = true,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                IsDeleted = false
            };

            _dbContext.Owners.Add(owner);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await EnsureCurrentPasswordRecordAsync(owner, passwordHashResult, cancellationToken);
            return owner;
        }

        var hasChanges = false;

        if (string.IsNullOrWhiteSpace(owner.DisplayName))
        {
            owner.DisplayName = displayName;
            hasChanges = true;
        }

        if (owner.Status != OwnerAccountStatus.Active)
        {
            owner.Status = OwnerAccountStatus.Active;
            hasChanges = true;
        }

        if (!owner.IsEmailConfirmed)
        {
            owner.IsEmailConfirmed = true;
            hasChanges = true;
        }

        if (!owner.IsActive)
        {
            owner.IsActive = true;
            hasChanges = true;
        }

        if (hasChanges)
        {
            owner.UpdatedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureCurrentPasswordRecordAsync(owner, null, cancellationToken);
        return owner;
    }

    private async Task EnsurePermissionsAsync(
        OwnerRole administratorRole,
        OwnerRole registeredOwnerRole,
        CancellationToken cancellationToken)
    {
        await _dbContext.Entry(administratorRole).Collection(entity => entity.PermissionRecords).LoadAsync(cancellationToken);
        await _dbContext.Entry(registeredOwnerRole).Collection(entity => entity.PermissionRecords).LoadAsync(cancellationToken);

        var hasChanges = false;

        foreach (var definition in GropMngPermissionProvider.GetAllPermissions())
        {
            var permission = await _dbContext.PermissionRecords.FirstOrDefaultAsync(
                entity => entity.SystemName == definition.SystemName,
                cancellationToken);

            if (permission is null)
            {
                permission = definition.ToEntity();
                _dbContext.PermissionRecords.Add(permission);
                hasChanges = true;
            }

            if (administratorRole.PermissionRecords.All(entity => entity.SystemName != definition.SystemName))
            {
                administratorRole.PermissionRecords.Add(permission);
                hasChanges = true;
            }

            if (definition.AssignToRegisteredOwnerByDefault
                && registeredOwnerRole.PermissionRecords.All(entity => entity.SystemName != definition.SystemName))
            {
                registeredOwnerRole.PermissionRecords.Add(permission);
                hasChanges = true;
            }
        }

        if (hasChanges)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAdministratorMembershipAsync(Owner owner, OwnerRole administratorRole, CancellationToken cancellationToken)
    {
        await _dbContext.Entry(owner).Collection(entity => entity.OwnerRoles).LoadAsync(cancellationToken);
        if (owner.OwnerRoles.Any(role => role.Id == administratorRole.Id))
            return;

        owner.OwnerRoles.Add(administratorRole);
        owner.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCurrentPasswordRecordAsync(
        Owner owner,
        PasswordHashResult? passwordHashResult,
        CancellationToken cancellationToken)
    {
        var hasCurrentPassword = await _dbContext.OwnerPasswords.AnyAsync(
            entity => entity.OwnerId == owner.Id && entity.IsCurrent,
            cancellationToken);

        if (hasCurrentPassword)
            return;

        var effectiveHash = passwordHashResult?.Hash ?? owner.PasswordHash;
        var effectiveSalt = passwordHashResult?.Salt ?? string.Empty;

        _dbContext.OwnerPasswords.Add(new OwnerPassword
        {
            OwnerId = owner.Id,
            PasswordHash = effectiveHash,
            PasswordSalt = effectiveSalt,
            CreatedAtUtc = DateTime.UtcNow,
            IsCurrent = true
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}