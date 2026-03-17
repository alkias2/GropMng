using GropMng.Core;

namespace GropMng.Core.Domain.Configuration;

public partial class Setting : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
