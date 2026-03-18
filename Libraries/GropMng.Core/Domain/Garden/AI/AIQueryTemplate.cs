using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.AI;

public partial class AIQueryTemplate : AuditableEntity
{
    public required string TemplateName { get; set; }

    public AiQueryScenario Scenario { get; set; } = AiQueryScenario.General;

    public SupportedLanguage Language { get; set; } = SupportedLanguage.Greek;

    public required string PromptTemplate { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}