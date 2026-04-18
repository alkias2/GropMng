using GropMng.Core.Domain.Localization;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

/// <summary>
/// Seeds the baseline application languages required by the startup flow.
/// </summary>
internal sealed class LanguageSeeder
{
    private readonly GropContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public LanguageSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Seeds the baseline Greek and English languages and returns the resolved language entities.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The resolved baseline languages.</returns>
    public async Task<SeededLanguages> SeedAsync(CancellationToken cancellationToken = default)
    {
        var greekLanguage = await _dbContext.Languages.FirstOrDefaultAsync(entity => entity.UniqueSeoCode == "el", cancellationToken);
        if (greekLanguage is null)
        {
            greekLanguage = new Language
            {
                Name = "Greek",
                LanguageCulture = "el-GR",
                UniqueSeoCode = "el",
                Published = true,
                DisplayOrder = 0,
                Rtl = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            _dbContext.Languages.Add(greekLanguage);
        }

        var englishLanguage = await _dbContext.Languages.FirstOrDefaultAsync(entity => entity.UniqueSeoCode == "en", cancellationToken);
        if (englishLanguage is null)
        {
            englishLanguage = new Language
            {
                Name = "English",
                LanguageCulture = "en-US",
                UniqueSeoCode = "en",
                Published = true,
                DisplayOrder = 1,
                Rtl = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            _dbContext.Languages.Add(englishLanguage);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SeededLanguages(greekLanguage, englishLanguage);
    }
}

/// <summary>
/// Represents the baseline languages resolved during startup seeding.
/// </summary>
/// <param name="GreekLanguage">The resolved Greek language entity.</param>
/// <param name="EnglishLanguage">The resolved English language entity.</param>
internal sealed record SeededLanguages(Language GreekLanguage, Language EnglishLanguage);