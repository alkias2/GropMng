using System.Security.Claims;
using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Services.Services.User;

/// <summary>
/// Implements the custom cookie-based owner sign-in and sign-out flow.
/// </summary>
public class OwnerAuthenticationService : IOwnerAuthenticationService
{
    internal const string OwnerSecurityContextByEmailCachePrefix = "Grop.auth.owner.byemail.";
    internal const string OwnerSecurityContextByIdCachePrefix = "Grop.auth.owner.byid.";

    private static readonly GropCacheKey OwnerSecurityContextByEmailCacheKey =
        new("Grop.auth.owner.byemail.v1.{0}", OwnerSecurityContextByEmailCachePrefix) { CacheTime = 5 };

    private static readonly GropCacheKey OwnerSecurityContextByIdCacheKey =
        new("Grop.auth.owner.byid.v1.{0}", OwnerSecurityContextByIdCachePrefix) { CacheTime = 5 };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<Owner> _ownerRepository;
    private readonly IOwnerPasswordService _ownerPasswordService;
    private readonly IGropStaticCacheManager _staticCacheManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerAuthenticationService"/> class.
    /// </summary>
    public OwnerAuthenticationService(
        IHttpContextAccessor httpContextAccessor,
        IRepository<Owner> ownerRepository,
        IOwnerPasswordService ownerPasswordService,
        IGropStaticCacheManager staticCacheManager)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _ownerPasswordService = ownerPasswordService ?? throw new ArgumentNullException(nameof(ownerPasswordService));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
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
        var owner = await LoadOwnerSecurityContextByEmailAsync(normalizedEmail, cancellationToken);
        if (owner is null)
            return null;

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

        // Invalidate security-context caches before the cookie is cleared so we still have the claims.
        var ownerIdClaim = httpContext.User?.FindFirstValue(CurrentOwnerProvider.OwnerIdClaimType);
        var emailClaim = httpContext.User?.FindFirstValue(ClaimTypes.Email);

        if (Guid.TryParse(ownerIdClaim, out var ownerId))
            await _staticCacheManager.RemoveAsync(OwnerSecurityContextByIdCacheKey, ownerId.ToString("N"));

        if (!string.IsNullOrWhiteSpace(emailClaim))
            await _staticCacheManager.RemoveAsync(OwnerSecurityContextByEmailCacheKey, emailClaim);

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

    private Task<Owner?> EnsureOwnerWithRolesAsync(Owner owner, CancellationToken cancellationToken)
    {
        if (owner.OwnerRoles.Count != 0)
            return Task.FromResult<Owner?>(owner);

        var cacheKey = _staticCacheManager.PrepareKey(OwnerSecurityContextByIdCacheKey, owner.OwnerId.ToString("N"));
        return _staticCacheManager.GetAsync<Owner?>(cacheKey, () => EnsureOwnerWithRolesCoreAsync(owner, cancellationToken));
    }

    private async Task<Owner?> EnsureOwnerWithRolesCoreAsync(Owner owner, CancellationToken cancellationToken)
    {
        var query = _ownerRepository.TableNoTracking
            .AsSplitQuery()
            .Where(entity => entity.OwnerId == owner.OwnerId)
            .Select(entity => new OwnerSecuritySnapshot
            {
                OwnerId = entity.OwnerId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DisplayName = entity.DisplayName,
                Email = entity.Email,
                PasswordHash = entity.PasswordHash,
                Status = entity.Status,
                IsActive = entity.IsActive,
                OwnerRoles = entity.OwnerRoles
                    .Select(role => new OwnerRoleSnapshot
                    {
                        SystemName = role.SystemName,
                        IsActive = role.IsActive
                    })
                    .ToList()
            })
            .Select(snapshot => snapshot.ToOwner());

        if (query is IAsyncEnumerable<Owner>)
        {
            try
            {
                return await query.FirstOrDefaultAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Fallback for unit tests that provide non-EF IQueryable providers.
            }
        }

        return query.FirstOrDefault();
    }

    private Task<Owner?> LoadOwnerSecurityContextByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var cacheKey = _staticCacheManager.PrepareKey(OwnerSecurityContextByEmailCacheKey, normalizedEmail);
        return _staticCacheManager.GetAsync<Owner?>(cacheKey, () => LoadOwnerSecurityContextByEmailCoreAsync(normalizedEmail, cancellationToken));
    }

    private async Task<Owner?> LoadOwnerSecurityContextByEmailCoreAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var query = _ownerRepository.TableNoTracking
            .AsSplitQuery()
            .Where(entity => entity.Email == normalizedEmail && entity.IsActive)
            .Select(entity => new OwnerSecuritySnapshot
            {
                OwnerId = entity.OwnerId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                DisplayName = entity.DisplayName,
                Email = entity.Email,
                PasswordHash = entity.PasswordHash,
                Status = entity.Status,
                IsActive = entity.IsActive,
                OwnerRoles = entity.OwnerRoles
                    .Select(role => new OwnerRoleSnapshot
                    {
                        SystemName = role.SystemName,
                        IsActive = role.IsActive
                    })
                    .ToList(),
                Passwords = entity.Passwords
                    .Select(passwordEntry => new OwnerPasswordSnapshot
                    {
                        PasswordHash = passwordEntry.PasswordHash,
                        PasswordSalt = passwordEntry.PasswordSalt,
                        CreatedAtUtc = passwordEntry.CreatedAtUtc,
                        IsCurrent = passwordEntry.IsCurrent
                    })
                    .ToList()
            })
            .Select(snapshot => snapshot.ToOwner());

        if (query is IAsyncEnumerable<Owner>)
        {
            try
            {
                return await query.FirstOrDefaultAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Fallback for unit tests that provide non-EF IQueryable providers.
            }
        }

        return query.FirstOrDefault();
    }

    private sealed class OwnerSecuritySnapshot
    {
        public Guid OwnerId { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string PasswordHash { get; init; } = string.Empty;

        public OwnerAccountStatus Status { get; init; }

        public bool IsActive { get; init; }

        public List<OwnerRoleSnapshot> OwnerRoles { get; init; } = [];

        public List<OwnerPasswordSnapshot> Passwords { get; init; } = [];

        public Owner ToOwner()
        {
            return new Owner
            {
                OwnerId = OwnerId,
                FirstName = FirstName,
                LastName = LastName,
                DisplayName = DisplayName,
                Email = Email,
                PasswordHash = PasswordHash,
                Status = Status,
                IsActive = IsActive,
                OwnerRoles = OwnerRoles
                    .Select(role => new OwnerRole
                    {
                        SystemName = role.SystemName,
                        IsActive = role.IsActive
                    })
                    .ToList(),
                Passwords = Passwords
                    .Select(passwordEntry => new OwnerPassword
                    {
                        PasswordHash = passwordEntry.PasswordHash,
                        PasswordSalt = passwordEntry.PasswordSalt,
                        CreatedAtUtc = passwordEntry.CreatedAtUtc,
                        IsCurrent = passwordEntry.IsCurrent
                    })
                    .ToList()
            };
        }
    }

    private sealed class OwnerRoleSnapshot
    {
        public string SystemName { get; init; } = string.Empty;

        public bool IsActive { get; init; }
    }

    private sealed class OwnerPasswordSnapshot
    {
        public string PasswordHash { get; init; } = string.Empty;

        public string PasswordSalt { get; init; } = string.Empty;

        public DateTime CreatedAtUtc { get; init; }

        public bool IsCurrent { get; init; }
    }
}
