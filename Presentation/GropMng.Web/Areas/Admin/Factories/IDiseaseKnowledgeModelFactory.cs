using GropMng.Web.Areas.Admin.Models;

namespace GropMng.Web.Areas.Admin.Factories;

/// <summary>
/// Defines the contract for preparing view models for the disease knowledge base admin area.
/// </summary>
public interface IDiseaseKnowledgeModelFactory
{
    /// <summary>
    /// Prepares the list model for the DataTable grid.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The list model with all non-deleted knowledge base entries.</returns>
    Task<List<DiseaseKnowledgeListModel>> PrepareListModelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares the create form model.
    /// </summary>
    /// <param name="fromNotificationId">Optional notification ID to pre-fill the common name from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edit model initialized for creation.</returns>
    Task<DiseaseKnowledgeEditModel> PrepareCreateModelAsync(int? fromNotificationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares the edit form model for an existing knowledge base entry.
    /// </summary>
    /// <param name="id">The knowledge base entry identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edit model, or <c>null</c> if the entry does not exist.</returns>
    Task<DiseaseKnowledgeEditModel?> PrepareEditModelAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new knowledge base entry from the edit model.
    /// </summary>
    /// <param name="model">The edit model containing the form data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created entry identifier.</returns>
    Task<int> SaveCreateAsync(DiseaseKnowledgeEditModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing knowledge base entry from the edit model.
    /// </summary>
    /// <param name="model">The edit model containing the form data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the update succeeded.</returns>
    Task<bool> SaveEditAsync(DiseaseKnowledgeEditModel model, CancellationToken cancellationToken = default);
}