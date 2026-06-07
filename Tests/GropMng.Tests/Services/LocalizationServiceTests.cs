using System.Text;
using System.Xml;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Caching;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Services.Caching;
using GropMng.Services.Services.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Covers XML import/export behavior for localization resources.
/// </summary>
public class LocalizationServiceTests
{
    [Fact]
    public async Task ExportResourcesToXmlAsync_WhenLanguageHasResources_ReturnsExistingGropSchema()
    {
        // Arrange
        var language = CreateLanguage();
        var resources = new List<LocaleStringResource>
        {
            new()
            {
                Id = 1,
                LanguageId = language.Id,
                ResourceName = "admin.sample.one",
                ResourceValue = "Value One"
            },
            new()
            {
                Id = 2,
                LanguageId = language.Id,
                ResourceName = "admin.sample.two",
                ResourceValue = "Value Two"
            }
        };

        var service = CreateLocalizationService(resources, language);

        // Act
        var xml = await service.ExportResourcesToXmlAsync(language);

        // Assert
        Assert.StartsWith("<?xml", xml, StringComparison.Ordinal);
        Assert.Contains("<Language", xml);
        Assert.Contains("Name=\"English\"", xml);
        Assert.Contains("LanguageCulture=\"en-US\"", xml);
        Assert.Contains("UniqueSeoCode=\"en\"", xml);
        Assert.Contains("<LocaleResource>", xml);
        Assert.Contains("<Name>admin.sample.one</Name>", xml);
        Assert.Contains("<Value>Value One</Value>", xml);
    }

    [Fact]
    public async Task ExportResourcesToXmlAsync_WhenExportedXmlIsImported_RoundtripSucceeds()
    {
        // Arrange
        var language = CreateLanguage();
        var sourceResources = new List<LocaleStringResource>
        {
            new()
            {
                Id = 1,
                LanguageId = language.Id,
                ResourceName = "common.save",
                ResourceValue = "Save"
            },
            new()
            {
                Id = 2,
                LanguageId = language.Id,
                ResourceName = "common.cancel",
                ResourceValue = "Cancel"
            }
        };

        var exportService = CreateLocalizationService(sourceResources, language);
        var importedResources = new List<LocaleStringResource>();
        var importService = CreateLocalizationService(importedResources, language);

        var exportedXml = await exportService.ExportResourcesToXmlAsync(language);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(exportedXml));
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Act
        await importService.ImportResourcesFromXmlAsync(language, reader, updateExistingResources: true);

        // Assert
        Assert.Equal(2, importedResources.Count);
        Assert.Equal("Save", importedResources.Single(r => r.ResourceName == "common.save").ResourceValue);
        Assert.Equal("Cancel", importedResources.Single(r => r.ResourceName == "common.cancel").ResourceValue);
    }

    [Fact]
    public async Task ImportResourcesFromXmlAsync_WhenKeysExistAndMissing_UpdatesCreatesAndInvalidatesCache()
    {
        // Arrange
        var language = CreateLanguage();
        var resources = new List<LocaleStringResource>
        {
            new()
            {
                Id = 7,
                LanguageId = language.Id,
                ResourceName = "admin.localization.existing",
                ResourceValue = "Old value",
                CreatedOnUtc = DateTime.UtcNow.AddDays(-2),
                UpdatedOnUtc = DateTime.UtcNow.AddDays(-2)
            }
        };

        var service = CreateLocalizationService(resources, language);

        var cachedBeforeImport = await service.GetAllResourcesByLanguageAsync(language.Id);
        Assert.Equal("Old value", cachedBeforeImport["admin.localization.existing"]);

        const string xml = """
<Language Name="English" LanguageCulture="en-US" UniqueSeoCode="en">
  <LocaleResource>
    <Name>admin.localization.existing</Name>
    <Value>Updated value</Value>
  </LocaleResource>
  <LocaleResource>
    <Name>admin.localization.created</Name>
    <Value>Created value</Value>
  </LocaleResource>
</Language>
""";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Act
        await service.ImportResourcesFromXmlAsync(language, reader, updateExistingResources: true);

        // Assert
        Assert.Equal("Updated value", resources.Single(r => r.ResourceName == "admin.localization.existing").ResourceValue);
        Assert.Equal("Created value", resources.Single(r => r.ResourceName == "admin.localization.created").ResourceValue);

        var reloaded = await service.GetAllResourcesByLanguageAsync(language.Id);
        Assert.Equal("Updated value", reloaded["admin.localization.existing"]);
        Assert.Equal("Created value", reloaded["admin.localization.created"]);
    }

    [Fact]
    public async Task ImportResourcesFromXmlAsync_WhenDuplicateKeysExist_ThrowsDomainException()
    {
        // Arrange
        var language = CreateLanguage();
        var service = CreateLocalizationService([], language);

        const string xml = """
<Language Name="English" LanguageCulture="en-US" UniqueSeoCode="en">
  <LocaleResource>
    <Name>admin.localization.same</Name>
    <Value>Value 1</Value>
  </LocaleResource>
  <LocaleResource>
    <Name>ADMIN.LOCALIZATION.SAME</Name>
    <Value>Value 2</Value>
  </LocaleResource>
</Language>
""";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() => service.ImportResourcesFromXmlAsync(language, reader, updateExistingResources: true));

        // Assert
        Assert.Contains("Duplicate locale resource keys", exception.Message);
        Assert.Contains("admin.localization.same", exception.Message);
    }

    [Fact]
    public async Task ImportResourcesFromXmlAsync_WhenXmlIsMalformed_ThrowsXmlException()
    {
        // Arrange
        var language = CreateLanguage();
        var service = CreateLocalizationService([], language);

        const string xml = "<Language><LocaleResource><Name>broken</Name><Value>oops</Value>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var reader = new StreamReader(stream, Encoding.UTF8);

        // Act / Assert
        await Assert.ThrowsAsync<XmlException>(() => service.ImportResourcesFromXmlAsync(language, reader, updateExistingResources: true));
    }

    private static LocalizationService CreateLocalizationService(List<LocaleStringResource> resources, Language language)
    {
        var resourceRepository = new Mock<IRepository<LocaleStringResource>>();
        var languageService = new Mock<ILanguageService>();
        var currentLanguageContext = new Mock<ICurrentLanguageContext>();
        var staticCacheManager = new GropMemoryCacheManager(
            new MemoryCache(new MemoryCacheOptions()),
            new GropCacheKeyManager(),
            NullLogger<GropMemoryCacheManager>.Instance);
        var logger = Mock.Of<ILogger<LocalizationService>>();

        resourceRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<Func<IQueryable<LocaleStringResource>, IQueryable<LocaleStringResource>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Func<IQueryable<LocaleStringResource>, IQueryable<LocaleStringResource>>? queryShaper, bool _, CancellationToken _) =>
            {
                IQueryable<LocaleStringResource> query = resources.AsQueryable();
                if (queryShaper is not null)
                    query = queryShaper(query);

                return (IReadOnlyList<LocaleStringResource>)query.ToList();
            });

        resourceRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<LocaleStringResource>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocaleStringResource resource, bool _, CancellationToken _) =>
            {
                resource.Id = resources.Count == 0 ? 1 : resources.Max(item => item.Id) + 1;
                resources.Add(resource);
                return resource;
            });

        resourceRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<LocaleStringResource>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LocaleStringResource resource, bool _, CancellationToken _) => resource);

        languageService
            .Setup(service => service.GetDefaultLanguageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(language);

        currentLanguageContext
            .Setup(context => context.GetCurrentLanguageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(language);

        return new LocalizationService(
            resourceRepository.Object,
            languageService.Object,
            currentLanguageContext.Object,
            staticCacheManager,
            logger);
    }

    private static Language CreateLanguage()
    {
        return new Language
        {
            Id = 1,
            Name = "English",
            LanguageCulture = "en-US",
            UniqueSeoCode = "en",
            Published = true,
            DisplayOrder = 1,
            CreatedOnUtc = DateTime.UtcNow.AddDays(-10),
            UpdatedOnUtc = DateTime.UtcNow.AddDays(-1)
        };
    }
}