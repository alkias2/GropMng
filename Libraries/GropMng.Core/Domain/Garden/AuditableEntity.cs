using GropMng.Core;

namespace GropMng.Core.Domain.Garden;

public abstract partial class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }
}