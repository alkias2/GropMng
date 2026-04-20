using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Web.Areas.Admin.Models.Owner;
using GropMng.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GropMng.Web.Areas.Admin.Factories.User;

/// <summary>
/// Default owner-account admin model factory.
/// </summary>
public class OwnerModelFactory : IOwnerModelFactory
{
    private const string AdministratorRoleSystemName = "Administrator";

    private readonly IRepository<Owner> _ownerRepository;
    private readonly IRepository<OwnerRole> _ownerRoleRepository;

    public OwnerModelFactory(IRepository<Owner> ownerRepository, IRepository<OwnerRole> ownerRoleRepository)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _ownerRoleRepository = ownerRoleRepository ?? throw new ArgumentNullException(nameof(ownerRoleRepository));
    }

    public Task<OwnerSearchModel> PrepareSearchModelAsync(OwnerSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new OwnerSearchModel();
        searchModel.SetGridPageSize();
        searchModel.AvailableStatuses = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "All statuses" },
            new() { Value = OwnerAccountStatus.Active.ToString(), Text = "Active" },
            new() { Value = OwnerAccountStatus.PendingActivation.ToString(), Text = "Pending activation" },
            new() { Value = OwnerAccountStatus.Inactive.ToString(), Text = "Inactive" }
        };

        return Task.FromResult(searchModel);
    }

    public async Task<OwnerListModel> PrepareListModelAsync(OwnerSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = Math.Max(searchModel.Page - 1, 0);
        var term = searchModel.SearchTerm?.Trim();
        var hasStatus = Enum.TryParse<OwnerAccountStatus>(searchModel.Status, true, out var parsedStatus);

        var owners = await _ownerRepository.GetPagedAsync(query =>
        {
            var shaped = query.Include(entity => entity.OwnerRoles).AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                shaped = shaped.Where(entity =>
                    entity.Email.Contains(term) ||
                    entity.FirstName.Contains(term) ||
                    entity.LastName.Contains(term) ||
                    entity.DisplayName.Contains(term));
            }

            if (hasStatus)
                shaped = shaped.Where(entity => entity.Status == parsedStatus);

            return shaped
                .OrderBy(entity => entity.DisplayName)
                .ThenBy(entity => entity.Email);
        }, pageIndex, searchModel.PageSize, cancellationToken: cancellationToken);

        var rows = owners.Select(entity => new OwnerRowModel
        {
            OwnerId = entity.OwnerId.ToString(),
            DisplayName = string.IsNullOrWhiteSpace(entity.DisplayName)
                ? string.Concat(entity.FirstName, " ", entity.LastName).Trim()
                : entity.DisplayName,
            Email = entity.Email,
            Status = entity.Status.ToString(),
            RolesSummary = entity.OwnerRoles.Any()
                ? string.Join(", ", entity.OwnerRoles.Where(role => role.IsActive).Select(role => role.Name))
                : "None",
            IsActive = entity.IsActive,
            IsEmailConfirmed = entity.IsEmailConfirmed
        }).ToList();

        var listModel = new OwnerListModel();
        return listModel.PrepareToGrid(searchModel, owners, () => rows);
    }

    public async Task<OwnerEditModel?> PrepareEditModelAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var owner = await GetOwnerWithRolesAsync(ownerId, cancellationToken);
        if (owner is null)
            return null;

        var model = new OwnerEditModel
        {
            OwnerId = owner.OwnerId,
            FirstName = owner.FirstName,
            LastName = owner.LastName,
            DisplayName = owner.DisplayName,
            Email = owner.Email,
            Status = owner.Status,
            IsActive = owner.IsActive,
            IsEmailConfirmed = owner.IsEmailConfirmed,
            IsSystemAdministrator = owner.OwnerRoles.Any(role => string.Equals(role.SystemName, AdministratorRoleSystemName, StringComparison.OrdinalIgnoreCase)),
            SelectedRoleSystemNames = owner.OwnerRoles.Select(role => role.SystemName).ToList()
        };

        model.AvailableRoles = await BuildAvailableRolesAsync(model.SelectedRoleSystemNames, cancellationToken);
        return model;
    }

    public async Task<bool> SaveEditAsync(OwnerEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var owner = await GetOwnerWithRolesAsync(model.OwnerId, cancellationToken, asNoTracking: false);
        if (owner is null)
            return false;

        if (string.IsNullOrWhiteSpace(model.Email))
            throw new DomainException("Owner email is required.");

        var selectedRoleSystemNames = model.SelectedRoleSystemNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (selectedRoleSystemNames.Length == 0)
            throw new DomainException("At least one role must be assigned to the owner.");

        var removingAdministratorRole = owner.OwnerRoles.Any(role => string.Equals(role.SystemName, AdministratorRoleSystemName, StringComparison.OrdinalIgnoreCase))
            && !selectedRoleSystemNames.Contains(AdministratorRoleSystemName, StringComparer.OrdinalIgnoreCase);

        if (removingAdministratorRole || (!model.IsActive && owner.OwnerRoles.Any(role => string.Equals(role.SystemName, AdministratorRoleSystemName, StringComparison.OrdinalIgnoreCase))))
        {
            var remainingAdministrators = await _ownerRepository.CountAsync(
                entity => entity.OwnerId != owner.OwnerId
                          && entity.IsActive
                          && entity.OwnerRoles.Any(role => role.IsActive && role.SystemName == AdministratorRoleSystemName),
                cancellationToken: cancellationToken);

            if (remainingAdministrators == 0)
                throw new DomainException("At least one active administrator must remain assigned in the system.");
        }

        var selectedRoles = await _ownerRoleRepository.FindAsync(
            entity => selectedRoleSystemNames.Contains(entity.SystemName) && entity.IsActive,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        owner.FirstName = model.FirstName.Trim();
        owner.LastName = model.LastName.Trim();
        owner.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName)
            ? string.Concat(model.FirstName, " ", model.LastName).Trim()
            : model.DisplayName.Trim();
        owner.Email = model.Email.Trim();
        owner.Status = model.Status;
        owner.IsActive = model.IsActive;
        owner.IsEmailConfirmed = model.IsEmailConfirmed;
        owner.UpdatedAtUtc = DateTime.UtcNow;

        owner.OwnerRoles.Clear();
        foreach (var role in selectedRoles)
            owner.OwnerRoles.Add(role);

        await _ownerRepository.UpdateAsync(owner, cancellationToken: cancellationToken);
        return true;
    }

    private async Task<IList<SelectListItem>> BuildAvailableRolesAsync(ICollection<string> selectedRoleSystemNames, CancellationToken cancellationToken)
    {
        var roles = await _ownerRoleRepository.GetAllAsync(
            query => query
                .Where(entity => entity.IsActive)
                .OrderByDescending(entity => entity.IsSystemRole)
                .ThenBy(entity => entity.Name),
            cancellationToken: cancellationToken);

        return roles.Select(role => new SelectListItem
        {
            Value = role.SystemName,
            Text = role.Name,
            Selected = selectedRoleSystemNames.Contains(role.SystemName, StringComparer.OrdinalIgnoreCase)
        }).ToList();
    }

    private async Task<Owner?> GetOwnerWithRolesAsync(Guid ownerId, CancellationToken cancellationToken, bool asNoTracking = true)
    {
        var query = asNoTracking ? _ownerRepository.TableNoTracking : _ownerRepository.Table;
        var shaped = query.Include(entity => entity.OwnerRoles);

        if (shaped.Provider is IAsyncQueryProvider)
            return await shaped.FirstOrDefaultAsync(entity => entity.OwnerId == ownerId, cancellationToken);

        return shaped.FirstOrDefault(entity => entity.OwnerId == ownerId);
    }
}
