using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;

namespace GropMng.Services.Services.Localization;

/// <summary>
/// Default implementation of language management operations.
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly IRepository<Language> _languageRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageService"/> class.
    /// </summary>
    /// <param name="languageRepository">The language repository.</param>
    public LanguageService(IRepository<Language> languageRepository)
    {
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Language>> GetAllLanguagesAsync(bool showHidden = false, CancellationToken cancellationToken = default)
    {
        return _languageRepository.GetAllAsync(
            query =>
            {
                if (!showHidden)
                    query = query.Where(language => language.Published);

                return query.OrderBy(language => language.DisplayOrder).ThenBy(language => language.Id);
            },
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<Language?> GetLanguageByIdAsync(int languageId, CancellationToken cancellationToken = default)
    {
        if (languageId <= 0)
            return Task.FromResult<Language?>(null);

        return _languageRepository.GetByIdAsync(languageId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Language> GetDefaultLanguageAsync(CancellationToken cancellationToken = default)
    {
        var language = await _languageRepository.FirstOrDefaultAsync(
            entity => entity.Published,
            cancellationToken: cancellationToken);

        if (language is not null)
            return language;

        throw new DomainException("No published language was found.");
    }
}
