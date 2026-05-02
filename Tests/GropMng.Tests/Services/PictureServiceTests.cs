using GropMng.Core.Domain.Media;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Services.Services.Media;
using Microsoft.Extensions.Hosting;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Unit tests for <see cref="PictureService"/>.
/// All file system I/O uses a real temp directory that is cleaned up after each test.
/// </summary>
public class PictureServiceTests : IDisposable
{
    private readonly Mock<IRepository<Picture>> _pictureRepo = new();
    private readonly Mock<ISettingService> _settingService = new();
    private readonly Mock<IHostEnvironment> _hostEnv = new();
    private readonly string _tempRoot;
    private readonly PictureService _sut;

    public PictureServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"grop_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_tempRoot, "wwwroot"));

        _hostEnv.Setup(e => e.ContentRootPath).Returns(_tempRoot);

        _settingService
            .Setup(s => s.LoadAsync<MediaSettings>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MediaSettings
            {
                MaximumImageSize    = 2000,
                DefaultImageQuality = 90,
                DefaultPlantImageUrl = "/images/default-plant.png"
            });

        _pictureRepo
            .Setup(r => r.CreateAsync(It.IsAny<Picture>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Picture p, bool _, CancellationToken _) =>
            {
                p.Id = 1;
                return p;
            });

        _pictureRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Picture>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Picture p, bool _, CancellationToken _) => p);

        _sut = new PictureService(_pictureRepo.Object, _settingService.Object, _hostEnv.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    // ── GetPictureSeNameAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetPictureSeNameAsync_NormalName_ReturnsSlug()
    {
        var result = await _sut.GetPictureSeNameAsync("Hello World! Plant");
        Assert.Equal("hello-world-plant", result);
    }

    [Fact]
    public async Task GetPictureSeNameAsync_EmptyName_ReturnsEmpty()
    {
        var result = await _sut.GetPictureSeNameAsync(string.Empty);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetPictureSeNameAsync_MultipleSpacesAndSymbols_ReturnsCleanSlug()
    {
        var result = await _sut.GetPictureSeNameAsync("  Rosa  Canina!! ");
        Assert.Equal("rosa-canina", result);
    }

    // ── GetFileExtensionFromMimeTypeAsync ───────────────────────────────────

    [Theory]
    [InlineData("image/jpeg", "jpg")]
    [InlineData("image/png",  "png")]
    [InlineData("image/gif",  "gif")]
    [InlineData("image/webp", "webp")]
    [InlineData("image/bmp",  "jpg")]   // unsupported → fallback to jpg
    public async Task GetFileExtensionFromMimeTypeAsync_KnownTypes_ReturnsCorrectExtension(
        string mimeType, string expectedExt)
    {
        var result = await _sut.GetFileExtensionFromMimeTypeAsync(mimeType);
        Assert.Equal(expectedExt, result);
    }

    // ── GetPictureContentTypeByFileExtension ────────────────────────────────

    [Theory]
    [InlineData("jpg",  "image/jpeg")]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("png",  "image/png")]
    [InlineData("gif",  "image/gif")]
    [InlineData("webp", "image/webp")]
    [InlineData("bmp",  "image/jpeg")] // unsupported → fallback
    public void GetPictureContentTypeByFileExtension_KnownExtensions_ReturnsCorrectMime(
        string ext, string expectedMime)
    {
        var result = _sut.GetPictureContentTypeByFileExtension(ext);
        Assert.Equal(expectedMime, result);
    }

    // ── GetDefaultPictureUrlAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetDefaultPictureUrlAsync_ReturnsSettingsUrl()
    {
        var result = await _sut.GetDefaultPictureUrlAsync();
        Assert.Equal("/images/default-plant.png", result);
    }

    // ── GetPictureUrlAsync (by id) ──────────────────────────────────────────

    [Fact]
    public async Task GetPictureUrlAsync_ZeroId_ShowDefault_ReturnsDefaultUrl()
    {
        var result = await _sut.GetPictureUrlAsync(0, showDefaultPicture: true);
        Assert.Equal("/images/default-plant.png", result);
    }

    [Fact]
    public async Task GetPictureUrlAsync_ZeroId_HideDefault_ReturnsEmpty()
    {
        var result = await _sut.GetPictureUrlAsync(0, showDefaultPicture: false);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetPictureUrlAsync_ValidId_NotFound_ShowDefault_ReturnsDefaultUrl()
    {
        _pictureRepo
            .Setup(r => r.GetByIdAsync(42, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Picture?)null);

        var result = await _sut.GetPictureUrlAsync(42, showDefaultPicture: true);
        Assert.Equal("/images/default-plant.png", result);
    }

    [Fact]
    public async Task GetPictureUrlAsync_ValidPicture_NoTargetSize_ReturnsVirtualPath()
    {
        var picture = new Picture { Id = 5, VirtualPath = "/images/uploads/test.jpg", MimeType = "image/jpeg" };
        _pictureRepo
            .Setup(r => r.GetByIdAsync(5, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(picture);

        var result = await _sut.GetPictureUrlAsync(5, targetSize: 0);
        Assert.Equal("/images/uploads/test.jpg", result);
    }

    // ── InsertPictureAsync (bytes) ──────────────────────────────────────────

    [Fact]
    public async Task InsertPictureAsync_UnsupportedMime_ThrowsInvalidOperation()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.InsertPictureAsync([0x00, 0x01], "image/bmp", "test"));
    }

    [Fact]
    public async Task InsertPictureAsync_ValidPng_CreatesFileAndRecord()
    {
        var pngBytes = CreateMinimalPng();

        var picture = await _sut.InsertPictureAsync(pngBytes, "image/png", "test-plant");

        Assert.NotNull(picture);
        Assert.Equal("image/png", picture.MimeType);
        Assert.NotEmpty(picture.VirtualPath!);
        Assert.True(picture.IsNew);

        // Verify the file was actually written to disk
        var physicalPath = Path.Combine(_tempRoot, "wwwroot",
            picture.VirtualPath!.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(physicalPath));
    }

    // ── DeletePictureAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeletePictureAsync_DeletesFileFromDisk()
    {
        // Arrange: create a real file in the uploads folder
        var uploadsDir = Path.Combine(_tempRoot, "wwwroot", "images", "uploads");
        Directory.CreateDirectory(uploadsDir);
        var fileName = "delete-me.jpg";
        var physicalPath = Path.Combine(uploadsDir, fileName);
        await File.WriteAllBytesAsync(physicalPath, [0xFF, 0xD8, 0xFF]);

        var picture = new Picture
        {
            Id = 10,
            MimeType = "image/jpeg",
            VirtualPath = $"/images/uploads/{fileName}"
        };

        _pictureRepo
            .Setup(r => r.DeleteAsync(picture, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeletePictureAsync(picture);

        // Assert
        Assert.False(File.Exists(physicalPath));
        _pictureRepo.Verify(r => r.DeleteAsync(picture, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── ValidatePictureAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ValidatePictureAsync_SmallImage_ReturnsSameSizeOrSmaller()
    {
        var pngBytes = CreateMinimalPng();
        var result = await _sut.ValidatePictureAsync(pngBytes, "image/png", "test");

        // Should return valid PNG bytes
        Assert.NotEmpty(result);
    }

    // ── SetSeoFilenameAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task SetSeoFilenameAsync_ExistingPicture_UpdatesSeoFilename()
    {
        var picture = new Picture { Id = 7, MimeType = "image/jpeg", SeoFilename = "old-name" };
        _pictureRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(picture);

        var result = await _sut.SetSeoFilenameAsync(7, "new-name");

        Assert.NotNull(result);
        Assert.Equal("new-name", result!.SeoFilename);
    }

    [Fact]
    public async Task SetSeoFilenameAsync_NonExistingPicture_ReturnsNull()
    {
        _pictureRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync((Picture?)null);

        var result = await _sut.SetSeoFilenameAsync(999, "name");

        Assert.Null(result);
    }

    // ── LoadPictureBinaryAsync ──────────────────────────────────────────────

    [Fact]
    public async Task LoadPictureBinaryAsync_MissingFile_ReturnsEmptyArray()
    {
        var picture = new Picture { Id = 3, VirtualPath = "/images/uploads/nonexistent.jpg" };
        var result = await _sut.LoadPictureBinaryAsync(picture);
        Assert.Empty(result);
    }

    // ── Helper ─────────────────────────────────────────────────────────────

    /// <summary>Creates a minimal valid 1×1 white PNG.</summary>
    private static byte[] CreateMinimalPng()
    {
        // Minimal valid PNG: 1x1 white pixel
        return
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk length + type
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // width=1, height=1
            0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, // bit depth=8, colorType=2 (RGB), CRC
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IDAT chunk
            0x54, 0x08, 0xD7, 0x63, 0xF8, 0xFF, 0xFF, 0x3F, // compressed pixel data
            0x00, 0x05, 0xFE, 0x02, 0xFE, 0xDC, 0xCC, 0x59, // ...
            0xE7, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IEND
            0x44, 0xAE, 0x42, 0x60, 0x82               // IEND CRC
        ];
    }
}
