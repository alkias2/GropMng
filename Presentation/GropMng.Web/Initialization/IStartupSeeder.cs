namespace GropMng.Web.Initialization;

/// <summary>
/// Coordinates the baseline startup seed workflow for the web application.
/// </summary>
public interface IStartupSeeder
{
    /// <summary>
    /// Seeds the baseline startup data required by the application.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous seed operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken = default);
}