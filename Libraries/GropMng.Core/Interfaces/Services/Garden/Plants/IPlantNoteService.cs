using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Defines application services for plant notes scoped to a plant instance.
/// </summary>
public interface IPlantNoteService
{
    /// <summary>
    /// Retrieves all notes for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of plant notes.</returns>
    Task<IReadOnlyList<PlantNote>> GetNotesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a note to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="note">The note to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created plant note.</returns>
    Task<PlantNote> CreateNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a note that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="note">The note entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated plant note.</returns>
    Task<PlantNote> UpdateNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a note from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="noteId">The identifier of the note to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteNoteAsync(int plantInstanceId, int noteId, Guid ownerId, CancellationToken cancellationToken = default);
}