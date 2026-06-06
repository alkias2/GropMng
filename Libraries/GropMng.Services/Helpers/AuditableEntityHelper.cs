using GropMng.Core.Domain.Garden;

namespace GropMng.Services.Helpers;

/// <summary>
/// Shared audit stamp helpers for entity create and update operations.
/// </summary>
public static class AuditableEntityHelper
{
    /// <summary>
    /// Sets creation timestamps and soft-delete defaults on an auditable entity.
    /// </summary>
    /// <param name="entity">The entity being created.</param>
    public static void StampForCreate(AuditableEntity entity)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAtUtc = now;
        entity.UpdatedAtUtc = now;
        entity.IsDeleted = false;
        entity.DeletedAtUtc = null;
    }

    /// <summary>
    /// Updates the modification timestamp on an auditable entity.
    /// </summary>
    /// <param name="entity">The entity being modified.</param>
    public static void StampForUpdate(AuditableEntity entity)
        => entity.UpdatedAtUtc = DateTime.UtcNow;
}