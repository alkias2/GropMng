using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides owner-scoped CRUD for plant notes tied to a plant instance.
/// </summary>
public class PlantNoteService : IPlantNoteService
{
    #region Fields

    private readonly IRepository<PlantNote> _plantNoteRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public PlantNoteService(
        IRepository<PlantNote> plantNoteRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _plantNoteRepository = plantNoteRepository ?? throw new ArgumentNullException(nameof(plantNoteRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantNote>> GetNotesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            PlantCacheDefaults.PlantNotesByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _plantNoteRepository.GetAllAsync(
                query => query
                    .Where(note => note.PlantInstanceId == plantInstanceId && note.OwnerId == ownerId)
                    .OrderByDescending(note => note.Id),
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<PlantNote> CreateNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        ValidateRequired(note.RichHtmlContent, nameof(note.RichHtmlContent));

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, note.OwnerId, cancellationToken);
        note.PlantInstanceId = plantInstanceId;
        note.OwnerId = plantInstance.OwnerId;
        note.Title = note.Title?.Trim();
        note.RichHtmlContent = note.RichHtmlContent.Trim();
        note.Tags = note.Tags?.Trim();
        AuditableEntityHelper.StampForCreate(note);

        var created = await _plantNoteRepository.CreateAsync(note, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantNotePrefix);

        return created;
    }

    /// <inheritdoc />
    public async Task<PlantNote> UpdateNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        ValidateRequired(note.RichHtmlContent, nameof(note.RichHtmlContent));

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, note.OwnerId, cancellationToken);
        var existingNote = await EnsureNoteOwnedAsync(plantInstanceId, note.Id, note.OwnerId, cancellationToken);
        existingNote.Title = note.Title?.Trim();
        existingNote.RichHtmlContent = note.RichHtmlContent.Trim();
        existingNote.Tags = note.Tags?.Trim();
        AuditableEntityHelper.StampForUpdate(existingNote);

        var updated = await _plantNoteRepository.UpdateAsync(existingNote, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantNotePrefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteNoteAsync(int plantInstanceId, int noteId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var note = await EnsureNoteOwnedAsync(plantInstanceId, noteId, ownerId, cancellationToken);
        await _plantNoteRepository.DeleteAsync(note, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantNotePrefix);
    }

    #endregion

    #region Private

    private async Task<PlantInstance> EnsurePlantInstanceOwnedAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        var plantInstance = await _plantInstanceRepository.FirstOrDefaultAsync(
            entity => entity.Id == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return plantInstance ?? throw new DomainException($"PlantInstance with id '{plantInstanceId}' was not found for owner '{ownerId}'.");
    }

    private async Task<PlantNote> EnsureNoteOwnedAsync(int plantInstanceId, int noteId, Guid ownerId, CancellationToken cancellationToken)
    {
        var note = await _plantNoteRepository.FirstOrDefaultAsync(
            entity => entity.Id == noteId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return note ?? throw new DomainException($"PlantNote with id '{noteId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
    }

    private static void ValidateRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{propertyName} is required.");
    }

    #endregion
}