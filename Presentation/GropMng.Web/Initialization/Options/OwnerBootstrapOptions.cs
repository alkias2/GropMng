namespace GropMng.Web.Initialization.Options;

/// <summary>
/// Configuration values used to bootstrap the initial administrator owner.
/// </summary>
public class OwnerBootstrapOptions
{
    public const string SectionName = "OwnerBootstrap";

    public string AdministratorEmail { get; set; } = "owner@gropmng.local";

    public string AdministratorPassword { get; set; } = "ChangeMe123!";

    public string AdministratorFirstName { get; set; } = "System";

    public string AdministratorLastName { get; set; } = "Administrator";

    public string AdministratorDisplayName { get; set; } = "System Administrator";
}
