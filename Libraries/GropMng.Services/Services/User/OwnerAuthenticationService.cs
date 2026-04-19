using System.Security.Claims;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GropMng.Services.Services.User;

/// <summary>
/// Implements the custom cookie-based owner sign-in and sign-out flow.
/// </summary>
public class OwnerAuthenticationService : IOwnerAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<Owner> _ownerRepository;
    private readonly IOwnerPasswordService _ownerPasswordService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerAuthenticationService"/> class.
    /// </summary>
    public OwnerAuthenticationService(
        IHttpContextAccessor httpContextAccessor,
        IRepository<Owner> ownerRepository,
        IOwnerPasswordService ownerPasswordService)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _ownerPasswordService = ownerPasswordService ?? throw new ArgumentNullException(nameof(ownerPasswordService));
    }

    /// <inheritdoc />
    public async Task SignInAsync(Owner owner, bool isPersistent = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new DomainException("Unable to sign in without an active HTTP context.");

        var authenticatedOwner = await EnsureOwnerWithRolesAsync(owner, cancellationToken) ?? owner;
        var displayName = string.IsNullOrWhiteSpace(authenticatedOwner.DisplayName)
            ? string.Join(' ', new[] { authenticatedOwner.FirstName, authenticatedOwner.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)))
            : authenticatedOwner.DisplayName;

        var claims = new List<Claim>
        {
            new(CurrentOwnerProvider.OwnerIdClaimType, authenticatedOwner.OwnerId.ToString()),
            new(ClaimTypes.NameIdentifier, authenticatedOwner.OwnerId.ToString()),
            new(ClaimTypes.Name, string.IsNullOrWhiteSpace(displayName) ? authenticatedOwner.Email : displayName),
            new(ClaimTypes.Email, authenticatedOwner.Email)
        };

        foreach (var role in authenticatedOwner.OwnerRoles.Where(role => role.IsActive))
            claims.Add(new Claim(ClaimTypes.Role, role.SystemName));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var properties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    /// <inheritdoc />
    public async Task<Owner?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalizedEmail = email.Trim();
        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.Email == normalizedEmail && entity.IsActive,
            cancellationToken: cancellationToken);

        owner ??= await LoadOwnerSecurityContextByEmailAsync(normalizedEmail, cancellationToken);
        if (owner is null)
            return null;

        if (owner.Passwords.Count == 0 || owner.OwnerRoles.Count == 0)
            owner = await LoadOwnerSecurityContextByEmailAsync(normalizedEmail, cancellationToken) ?? owner;

        if (owner.Status != OwnerAccountStatus.Active || !owner.IsActive)
            return null;

        var currentPassword = owner.Passwords
            .Where(entity => entity.IsCurrent)
            .OrderByDescending(entity => entity.CreatedAtUtc)
            .FirstOrDefault();

        var storedHash = currentPassword?.PasswordHash ?? owner.PasswordHash;
        var storedSalt = currentPassword?.PasswordSalt ?? string.Empty;

        return _ownerPasswordService.VerifyPassword(password, storedHash, storedSalt)
            ? owner
            : null;
    }

    /// <inheritdoc />
    public async Task SignOutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new DomainException("Unable to sign out without an active HTTP context.");

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <inheritdoc />
    public async Task<Owner?> GetAuthenticatedOwnerAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var ownerIdValue = httpContext.User.FindFirstValue(CurrentOwnerProvider.OwnerIdClaimType);
        if (!Guid.TryParse(ownerIdValue, out var ownerId))
            return null;

        return await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.OwnerId == ownerId && entity.IsActive,
            cancellationToken: cancellationToken);
    }

    private async Task<Owner?> EnsureOwnerWithRolesAsync(Owner owner, CancellationToken cancellationToken)
    {
        if (owner.OwnerRoles.Count != 0)
            return owner;

        var query = _ownerRepository.TableNoTracking
            .Include(entity => entity.OwnerRoles)
                .ThenInclude(role => role.PermissionRecords);

        if (query.Provider is IAsyncQueryProvider)
            return await query.FirstOrDefaultAsync(entity => entity.OwnerId == owner.OwnerId, cancellationToken);

        return query.FirstOrDefault(entity => entity.OwnerId == owner.OwnerId);
    }

    private async Task<Owner?> LoadOwnerSecurityContextByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var query = _ownerRepository.TableNoTracking
            .Include(entity => entity.OwnerRoles)
                .ThenInclude(role => role.PermissionRecords)
            .Include(entity => entity.Passwords);

        if (query.Provider is IAsyncQueryProvider)
            return await query.FirstOrDefaultAsync(entity => entity.Email == normalizedEmail && entity.IsActive, cancellationToken);

        return query.FirstOrDefault(entity => entity.Email == normalizedEmail && entity.IsActive);
    }
}
