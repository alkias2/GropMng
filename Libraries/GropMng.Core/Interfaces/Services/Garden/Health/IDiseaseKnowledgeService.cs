using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Core.Interfaces.Services.Garden.Health;

/// <summary>
/// Provides operations for managing the disease knowledge base (admin encyclopedia).
/// </summary>
public interface IDiseaseKnowledgeService
{
    /// <summary>
    /// Retrieves all non-deleted knowledge base entries.
    /// </summary>
    Task<List<DiseaseKnowledge>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single knowledge base entry by its identifier.
    /// </summary>
    Task<DiseaseKnowledge> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches knowledge base entries by common name or scientific name.
    /// When <paramref name="plantId"/> is provided, results are filtered to diseases linked to that plant type.
    /// </summary>
    Task<List<DiseaseKnowledge>> SearchAsync(string term, int? plantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new knowledge base entry.
    /// </summary>
    Task<DiseaseKnowledge> CreateAsync(DiseaseKnowledge knowledge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing knowledge base entry.
    /// </summary>
    Task<DiseaseKnowledge> UpdateAsync(DiseaseKnowledge knowledge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a knowledge base entry. Fails if any <see cref="PlantProblemRecord"/>
    /// still references this entry.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a photo to a knowledge base entry.
    /// </summary>
    Task<DiseaseKnowledgePhoto> AddPhotoAsync(DiseaseKnowledgePhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a photo from a knowledge base entry.
    /// </summary>
    Task DeletePhotoAsync(int id, CancellationToken cancellationToken = default);
}