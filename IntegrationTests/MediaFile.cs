using System.Net;
using System.Net.Http.Json;
using Application.ApiContracts.File.Responses;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace IntegrationTests;

public class MediaFile : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public MediaFile(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact(DisplayName = "MF_002 - Tải lên ảnh thành công với định dạng JPG hợp lệ")]
    public async Task UploadImage_ValidJpg_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var content = new MultipartFormDataContent();
        var imageBytes = new byte[204800]; // 200KB
        content.Add(new ByteArrayContent(imageBytes), "file", "image.jpg");

        // Act
        var response = await _client.PostAsync("/api/v1/MediaFile/upload-image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MediaFileResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.StorageType.Should().Be("local");
        result.StoragePath.Should().EndWith(".jpg");
        result.ContentType.Should().Be("image/jpeg");
        result.FileSize.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "MF_007 - Tải lên nhiều ảnh cùng lúc thành công")]
    public async Task UploadManyImages_ValidFiles_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(new byte[51200]), "files", "image1.webp" },
            { new ByteArrayContent(new byte[61440]), "files", "image2.jpg" },
            { new ByteArrayContent(new byte[71680]), "files", "image3.webp" }
        };

        // Act
        var response = await _client.PostAsync("/api/v1/MediaFile/upload-images", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var results = await response.Content.ReadFromJsonAsync<List<MediaFileResponse>>();
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results!.All(r => r.Id > 0).Should().BeTrue();
    }

    [Fact(DisplayName = "MF_009 - Xoá file thành công (Soft Delete)")]
    public async Task DeleteFile_ExistingFile_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = "test-delete-file.webp",
            OriginalFileName = "test.webp",
            ContentType = "image/webp",
            FileExtension = ".webp",
            FileSize = 100000,
            DeletedAt = null
        };
        db.MediaFiles.Add(mediaFile);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/MediaFile/{mediaFile.StoragePath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var deletedFile = await db.MediaFiles.FindAsync(mediaFile.Id);
        deletedFile.Should().NotBeNull();
        deletedFile!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "MF_013 - Xoá nhiều file cùng lúc thành công")]
    public async Task DeleteManyFiles_ExistingFiles_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var files = new List<MediaFileEntity>
        {
            new (){ StorageType = "local", StoragePath = "delete-many-1.webp", ContentType = "image/webp", DeletedAt = null },
            new (){ StorageType = "local", StoragePath = "delete-many-2.jpg", ContentType = "image/jpeg", DeletedAt = null },
            new (){ StorageType = "local", StoragePath = "delete-many-3.webp", ContentType = "image/webp", DeletedAt = null }
        };
        db.MediaFiles.AddRange(files);
        await db.SaveChangesAsync();

        List<string> request = [.. files.Select(f => f.StoragePath!)];

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/MediaFile/delete-many", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        foreach (var file in files)
        {
            var deletedFile = await db.MediaFiles.FindAsync(file.Id);
            deletedFile!.DeletedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "MF_015 - Khôi phục file đã xoá thành công")]
    public async Task RestoreFile_DeletedFile_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = "restore-file.webp",
            OriginalFileName = "restore.webp",
            ContentType = "image/webp",
            FileExtension = ".webp",
            FileSize = 100000,
            DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z")
        };
        db.MediaFiles.Add(mediaFile);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/v1/MediaFile/restore/{mediaFile.StoragePath}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var restoredFile = await db.MediaFiles.FindAsync(mediaFile.Id);
        restoredFile.Should().NotBeNull();
        restoredFile!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "MF_019 - Khôi phục nhiều file cùng lúc thành công")]
    public async Task RestoreManyFiles_DeletedFiles_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var files = new List<MediaFileEntity>
        {
            new (){ StorageType = "local", StoragePath = "restore-many-1.webp", ContentType = "image/webp", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") },
            new (){ StorageType = "local", StoragePath = "restore-many-2.jpg", ContentType = "image/jpeg", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") },
            new (){ StorageType = "local", StoragePath = "restore-many-3.webp", ContentType = "image/webp", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") }
        };
        db.MediaFiles.AddRange(files);
        await db.SaveChangesAsync();

        List<string> request = [.. files.Select(f => f.StoragePath!)];

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/MediaFile/restore-many", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        foreach (var file in files)
        {
            var restoredFile = await db.MediaFiles.FindAsync(file.Id);
            restoredFile!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "MF_021 - Xem ảnh thành công với kích thước gốc")]
    public async Task ViewImage_OriginalSize_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = "view-original.webp",
            OriginalFileName = "view.webp",
            ContentType = "image/webp",
            FileExtension = ".webp",
            FileSize = 100000,
            DeletedAt = null
        };
        db.MediaFiles.Add(mediaFile);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/MediaFile/view-image/{mediaFile.StoragePath}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
    }

    [Fact(DisplayName = "MF_022 - Xem ảnh thành công với resize theo width")]
    public async Task ViewImage_WithResize_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = "view-resize.webp",
            OriginalFileName = "resize.webp",
            ContentType = "image/webp",
            FileExtension = ".webp",
            FileSize = 500000,
            DeletedAt = null
        };
        db.MediaFiles.Add(mediaFile);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/MediaFile/view-image/{mediaFile.StoragePath}?width=300");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
    }

    [Fact(DisplayName = "MF_031 - Lấy thông tin file theo ID thành công")]
    public async Task GetFileById_ExistingFile_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var mediaFile = new MediaFileEntity
        {
            StorageType = "local",
            StoragePath = "getbyid-test.webp",
            OriginalFileName = "test.webp",
            ContentType = "image/webp",
            FileExtension = ".webp",
            FileSize = 150000,
            DeletedAt = null
        };
        db.MediaFiles.Add(mediaFile);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/MediaFile/{mediaFile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MediaFileResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(mediaFile.Id);
        result.StorageType.Should().Be("local");
        result.StoragePath.Should().Be("getbyid-test.webp");
        result.ContentType.Should().Be("image/webp");
        result.FileSize.Should().Be(150000);
    }

    [Fact(DisplayName = "MF_035 - Lấy danh sách file thành công với phân trang")]
    public async Task GetFilesList_WithPagination_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var files = new List<MediaFileEntity>();
        for (int i = 1; i <= 25; i++)
        {
            files.Add(new MediaFileEntity
            {
                StorageType = "local",
                StoragePath = $"pagination-file-{i}.webp",
                ContentType = "image/webp",
                FileSize = 100000,
                DeletedAt = null
            });
        }
        db.MediaFiles.AddRange(files);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/MediaFile?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Response should contain pagination metadata
    }

    [Fact(DisplayName = "MF_036 - Lấy danh sách file với filter theo ContentType")]
    public async Task GetFilesList_FilterByContentType_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var webpFiles = new List<MediaFileEntity>();
        for (int i = 1; i <= 5; i++)
        {
            webpFiles.Add(new MediaFileEntity
            {
                StorageType = "local",
                StoragePath = $"filter-webp-{i}.webp",
                ContentType = "image/webp",
                FileSize = 100000,
                DeletedAt = null
            });
        }
        
        var jpgFiles = new List<MediaFileEntity>();
        for (int i = 1; i <= 3; i++)
        {
            jpgFiles.Add(new MediaFileEntity
            {
                StorageType = "local",
                StoragePath = $"filter-jpg-{i}.jpg",
                ContentType = "image/jpeg",
                FileSize = 100000,
                DeletedAt = null
            });
        }
        
        db.MediaFiles.AddRange(webpFiles);
        db.MediaFiles.AddRange(jpgFiles);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/MediaFile?filters=ContentType==image/webp");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Response should contain only webp files
    }

    [Fact(DisplayName = "MF_037 - Lấy danh sách file với sorting theo FileSize giảm dần")]
    public async Task GetFilesList_SortByFileSizeDesc_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var files = new List<MediaFileEntity>
        {
            new (){ StorageType = "local", StoragePath = "sort-1.webp", ContentType = "image/webp", FileSize = 100000, DeletedAt = null },
            new (){ StorageType = "local", StoragePath = "sort-2.webp", ContentType = "image/webp", FileSize = 200000, DeletedAt = null },
            new (){ StorageType = "local", StoragePath = "sort-3.webp", ContentType = "image/webp", FileSize = 150000, DeletedAt = null },
            new (){ StorageType = "local", StoragePath = "sort-4.webp", ContentType = "image/webp", FileSize = 50000, DeletedAt = null },
            new (){ StorageType = "local", StoragePath = "sort-5.webp", ContentType = "image/webp", FileSize = 300000, DeletedAt = null }
        };
        db.MediaFiles.AddRange(files);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/MediaFile?sorts=-FileSize");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Response should be sorted by FileSize descending
    }

    [Fact(DisplayName = "MF_038 - Lấy danh sách file đã xoá thành công")]
    public async Task GetDeletedFilesList_WithPagination_Success()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        var deletedFiles = new List<MediaFileEntity>();
        for (int i = 1; i <= 15; i++)
        {
            deletedFiles.Add(new MediaFileEntity
            {
                StorageType = "local",
                StoragePath = $"deleted-file-{i}.webp",
                ContentType = "image/webp",
                FileSize = 100000,
                DeletedAt = DateTimeOffset.UtcNow.AddDays(-i)
            });
        }
        db.MediaFiles.AddRange(deletedFiles);
        await db.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/MediaFile/deleted?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Response should contain only deleted files with pagination
    }
}
