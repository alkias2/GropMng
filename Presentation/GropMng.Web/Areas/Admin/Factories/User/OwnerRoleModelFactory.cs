using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Domain.Security;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Web.Areas.Admin.Models.Roles;
using GropMng.Web.Framework.Models.Extensions;
using GropMng.Web.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GropMng.Web.Areas.Admin.Factories.User;

/// <summary>
/// Default owner-role admin model factory.
/// </summary>
public class OwnerRoleModelFactory : IOwnerRoleModelFactory
{
    private const string AdministratorRoleSystemName = "Administrator";

    private readonly IRepository<OwnerRole> _ownerRoleRepository;
    private readonly IRepository<PermissionRecord> _permissionRepository;

    public OwnerRoleModelFactory(IRepository<OwnerRole> ownerRoleRepository, IRepository<PermissionRecord> permissionRepository)
    {
        _ownerRoleRepository = ownerRoleRepository ?? throw new ArgumentNullException(nameof(ownerRoleRepository));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public Task<OwnerRoleSearchModel> PrepareSearchModelAsync(OwnerRoleSearchModel? searchModel = null, CancellationToken cancellationToken = default)
    {
        searchModel ??= new OwnerRoleSearchModel();
        searchModel.SetGridPageSize();
        return Task.FromResult(searchModel);
    }

    public async Task<OwnerRoleListModel> PrepareListModelAsync(OwnerRoleSearchModel searchModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var pageIndex = Math.Max(searchModel.Page - 1, 0);
        var term = searchModel.SearchTerm?.Trim();

        var roles = await _ownerRoleRepository.GetPagedAsync(query =>
        {
            var shaped = query.Include(entity => entity.PermissionRecords).AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                shaped = shaped.Where(entity =>
                    entity.Name.Contains(term) ||
                    entity.SystemName.Contains(term) ||
                    entity.Description.Contains(term));
            }

            return shaped
                .OrderByDescending(entity => entity.IsSystemRole)
                .ThenBy(entity => entity.Name);
        }, pageIndex, searchModel.PageSize, cancellationToken: cancellationToken);

        var rows = roles.Select(entity => new OwnerRoleRowModel
        {
            Id = entity.Id,
            Name = entity.Name,
            SystemName = entity.SystemName,
            Description = entity.Description,
            PermissionCount = entity.PermissionRecords.Count,
            PermissionsSummary = entity.PermissionRecords.Any()
                ? string.Join(", ", entity.PermissionRecords.Select(permission => permission.Name))
                : "No permissions assigned",
            IsActive = entity.IsActive,
            IsSystemRole = entity.IsSystemRole
        }).ToList();

        var listModel = new OwnerRoleListModel();
        return listModel.PrepareToGrid(searchModel, roles, () => rows);
    }

    public async Task<OwnerRoleEditModel?> PrepareEditModelAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await GetRoleWithPermissionsAsync(roleId, cancellationToken);
        if (role is null)
            return null;

        var selectedPermissions = role.PermissionRecords
            .Select(permission => permission.SystemName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new OwnerRoleEditModel
        {
            Id = role.Id,
            Name = role.Name,
            SystemName = role.SystemName,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystemRole = role.IsSystemRole,
            SelectedPermissionSystemNames = selectedPermissions.ToList(),
            PermissionGroups = BuildPermissionGroups(selectedPermissions)
        };
    }

    public async Task<bool> SaveEditAsync(OwnerRoleEditModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        var role = await GetRoleWithPermissionsAsync(model.Id, cancellationToken, asNoTracking: false);
        if (role is null)
            return false;

        if (role.IsSystemRole && string.Equals(role.SystemName, AdministratorRoleSystemName, StringComparison.OrdinalIgnoreCase) && !model.IsActive)
            throw new DomainException("The Administrator role cannot be deactivated.");

        if (!role.IsSystemRole)
        {
            role.Name = string.IsNullOrWhiteSpace(model.Name) ? role.Name : model.Name.Trim();
            role.SystemName = string.IsNullOrWhiteSpace(model.SystemName) ? role.SystemName : model.SystemName.Trim();
        }

        role.Description = model.Description?.Trim() ?? string.Empty;
        role.IsActive = model.IsActive;

        var selectedNames = model.SelectedPermissionSystemNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var existingPermissions = (await _permissionRepository.FindAsync(
            entity => selectedNames.Contains(entity.SystemName),
            asNoTracking: false,
            cancellationToken: cancellationToken)).ToList();

        var existingNames = existingPermissions.Select(permission => permission.SystemName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingDefinitions = GropMngPermissionProvider.GetAllPermissions()
            .Where(definition => selectedNames.Contains(definition.SystemName, StringComparer.OrdinalIgnoreCase) && !existingNames.Contains(definition.SystemName))
            .Select(definition => definition.ToEntity())
            .ToList();

        if (missingDefinitions.Count > 0)
        {
            await _permissionRepository.CreateAsync(missingDefinitions, saveNow: false, cancellationToken: cancellationToken);
            existingPermissions.AddRange(missingDefinitions);
        }

        role.PermissionRecords.Clear();
        foreach (var permission in existingPermissions)
            role.PermissionRecords.Add(permission);

        await _ownerRoleRepository.UpdateAsync(role, cancellationToken: cancellationToken);
        return true;
    }

    private static IList<PermissionGroupModel> BuildPermissionGroups(ISet<string> selectedPermissions)
    {
        return GropMngPermissionProvider.GetAllPermissions()
            .GroupBy(permission => permission.Category)
            .OrderBy(group => group.Key)
            .Select(group => new PermissionGroupModel
            {
                Category = group.Key,
                Permissions = group
                    .OrderBy(permission => permission.Name)
                    .Select(permission => new PermissionCheckboxModel
                    {
                        Name = permission.Name,
                        SystemName = permission.SystemName,
                        Selected = selectedPermissions.Contains(permission.SystemName)
                    })
                    .ToList()
            })
            .ToList();
    }

    private async Task<OwnerRole?> GetRoleWithPermissionsAsync(int roleId, CancellationToken cancellationToken, bool asNoTracking = true)
    {
        var query = asNoTracking ? _ownerRoleRepository.TableNoTracking : _ownerRoleRepository.Table;
        var shaped = query.Include(entity => entity.PermissionRecords);

        if (shaped.Provider is IAsyncQueryProvider)
            return await shaped.FirstOrDefaultAsync(entity => entity.Id == roleId, cancellationToken);

        return shaped.FirstOrDefault(entity => entity.Id == roleId);
    }
}
