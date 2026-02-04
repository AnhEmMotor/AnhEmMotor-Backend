using Application.ApiContracts.File.Responses;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Domain.Constants.Permission;
using System.Net.Http.Headers;
using MediaFileEntity = Domain.Entities.MediaFile;
using IntegrationTests.SetupClass;

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

#pragma warning disable CRR0035
    [Fact(DisplayName = "MF_002 - Tải lên ảnh thành công với định dạng JPG hợp lệ")]
    public async Task UploadImage_ValidJpg_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [PermissionsList.Products.Create],
            email);

        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var content = IntegrationTestFileHelper.CreateSingleImageForm();

        var response = await _client.PostAsync(
            "/api/v1/MediaFile/upload-image",
            content);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API returned {response.StatusCode}. Response Body: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content
            .ReadFromJsonAsync<MediaFileResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.StorageType.Should().Be("local");
        result.StoragePath.Should().EndWith(".webp");
        result.ContentType.Should().Be("image/webp");
        result.FileSize.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "MF_007 - Tải lên nhiều ảnh cùng lúc thành công")]
    public async Task UploadManyImages_ValidFiles_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Create], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var validBytes = IntegrationTestFileHelper.GetValidJpgBytes();
        var content = IntegrationTestFileHelper.CreateManyImagesForm(
            ("files", "image1.jpg", "image/jpeg", validBytes),
            ("files", "image2.jpg", "image/jpeg", validBytes),
            ("files", "image3.jpg", "image/jpeg", validBytes)
        );

        var response = await _client.PostAsync("/api/v1/MediaFile/upload-images", content).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var results = await response.Content
            .ReadFromJsonAsync<List<MediaFileResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results!.All(r => r.Id > 0).Should().BeTrue();
        results.All(r => r.ContentType == "image/webp").Should().BeTrue();
    }

    [Fact(DisplayName = "MF_009 - Xoá file thành công (Soft Delete)")]
    public async Task DeleteFile_ExistingFile_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

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
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.DeleteAsync($"/api/v1/MediaFile/{mediaFile.StoragePath}").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var deletedFile = await verifyDb.MediaFiles.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == mediaFile.Id, CancellationToken.None).ConfigureAwait(true);
        deletedFile.Should().NotBeNull();
        deletedFile!.DeletedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "MF_013 - Xoá nhiều file cùng lúc thành công")]
    public async Task DeleteManyFiles_ExistingFiles_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Edit, PermissionsList.Products.Create], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var validBytes = IntegrationTestFileHelper.GetValidJpgBytes();
        var content = IntegrationTestFileHelper.CreateManyImagesForm(
            ("files", "delete1.jpg", "image/jpeg", validBytes),
            ("files", "delete2.jpg", "image/jpeg", validBytes),
            ("files", "delete3.jpg", "image/jpeg", validBytes)
        );

        var uploadRes = await _client.PostAsync("/api/v1/MediaFile/upload-images", content);
        uploadRes.EnsureSuccessStatusCode();
        var uploadedFiles = await uploadRes.Content.ReadFromJsonAsync<List<MediaFileResponse>>();

        var requestBody = new { StoragePaths = uploadedFiles!.Select(f => f.StoragePath!).ToList() };

        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/MediaFile/delete-many")
        {
            Content = JsonContent.Create(requestBody)
        };
        var response = await _client.SendAsync(requestMessage).ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        
        foreach(var file in uploadedFiles!)
        {
            var deletedFile = await verifyDb.MediaFiles.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == file.Id, CancellationToken.None).ConfigureAwait(true);
            deletedFile.Should().NotBeNull();
            deletedFile!.DeletedAt.Should().NotBeNull();
        }
    }

    [Fact(DisplayName = "MF_015 - Khôi phục file đã xoá thành công")]
    public async Task RestoreFile_DeletedFile_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

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
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.PostAsync($"/api/v1/MediaFile/restore/{mediaFile.StoragePath}", null)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var restoredFile = await verifyDb.MediaFiles.FindAsync(mediaFile.Id, CancellationToken.None).ConfigureAwait(true);
        restoredFile.Should().NotBeNull();
        restoredFile!.DeletedAt.Should().BeNull();
    }

    [Fact(DisplayName = "MF_019 - Khôi phục nhiều file cùng lúc thành công")]
    public async Task RestoreManyFiles_DeletedFiles_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Edit, PermissionsList.Products.Create], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // 1. Upload files
        var validBytes = IntegrationTestFileHelper.GetValidJpgBytes();
        var content = IntegrationTestFileHelper.CreateManyImagesForm(
            ("files", "restore1.jpg", "image/jpeg", validBytes),
            ("files", "restore2.jpg", "image/jpeg", validBytes),
            ("files", "restore3.jpg", "image/jpeg", validBytes)
        );

        var uploadRes = await _client.PostAsync("/api/v1/MediaFile/upload-images", content);
        uploadRes.EnsureSuccessStatusCode();
        var uploadedFiles = await uploadRes.Content.ReadFromJsonAsync<List<MediaFileResponse>>();

        // 2. Delete files
        var requestBody = new { StoragePaths = uploadedFiles!.Select(f => f.StoragePath!).ToList() };
        var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/MediaFile/delete-many")
        {
            Content = JsonContent.Create(requestBody)
        };
        var deleteRes = await _client.SendAsync(deleteMessage);
        deleteRes.EnsureSuccessStatusCode();

        // 3. Restore files
        var restoreRes = await _client.PostAsJsonAsync("/api/v1/MediaFile/restore-many", requestBody).ConfigureAwait(true);

        restoreRes.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        foreach(var file in uploadedFiles!)
        {
            var restoredFile = await verifyDb.MediaFiles.FindAsync(file.Id, CancellationToken.None).ConfigureAwait(true);
            restoredFile.Should().NotBeNull();
            restoredFile!.DeletedAt.Should().BeNull();
        }
    }

    [Fact(DisplayName = "MF_021 - Xem ảnh thành công với kích thước gốc")]
    public async Task ViewImage_OriginalSize_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Create, PermissionsList.Products.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var content = IntegrationTestFileHelper.CreateSingleImageForm();
        var uploadRes = await _client.PostAsync("/api/v1/MediaFile/upload-image", content);
        uploadRes.EnsureSuccessStatusCode();
        var uploadedFile = await uploadRes.Content.ReadFromJsonAsync<MediaFileResponse>();

        var response = await _client.GetAsync(
            $"/api/v1/MediaFile/view-image/{uploadedFile!.StoragePath}",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
    }

    [Fact(DisplayName = "MF_022 - Xem ảnh thành công với resize theo width")]
    public async Task ViewImage_WithResize_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Create, PermissionsList.Products.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var content = IntegrationTestFileHelper.CreateSingleImageForm();
        var uploadRes = await _client.PostAsync("/api/v1/MediaFile/upload-image", content);
        uploadRes.EnsureSuccessStatusCode();
        var uploadedFile = await uploadRes.Content.ReadFromJsonAsync<MediaFileResponse>();

        var response = await _client.GetAsync(
            $"/api/v1/MediaFile/view-image/{uploadedFile!.StoragePath}?width=300",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("image/webp");
    }

    [Fact(DisplayName = "MF_031 - Lấy thông tin file theo ID thành công")]
    public async Task GetFileById_ExistingFile_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.Edit], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

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
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync($"/api/v1/MediaFile/{mediaFile.Id}", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content
            .ReadFromJsonAsync<MediaFileResponse>(CancellationToken.None)
            .ConfigureAwait(true);
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
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var files = new List<MediaFileEntity>();
        for(int i = 1; i <= 25; i++)
        {
            files.Add(
                new MediaFileEntity
                {
                    StorageType = "local",
                    StoragePath = $"pagination-file-{i}.webp",
                    ContentType = "image/webp",
                    FileSize = 100000,
                    DeletedAt = null
                });
        }
        db.MediaFiles.AddRange(files);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/MediaFile?page=1&pageSize=10").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "MF_036 - Lấy danh sách file với filter theo ContentType")]
    public async Task GetFilesList_FilterByContentType_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var webpFiles = new List<MediaFileEntity>();
        for(int i = 1; i <= 5; i++)
        {
            webpFiles.Add(
                new MediaFileEntity
                {
                    StorageType = "local",
                    StoragePath = $"filter-webp-{i}.webp",
                    ContentType = "image/webp",
                    FileSize = 100000,
                    DeletedAt = null
                });
        }

        var jpgFiles = new List<MediaFileEntity>();
        for(int i = 1; i <= 3; i++)
        {
            jpgFiles.Add(
                new MediaFileEntity
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
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync(
            "/api/v1/MediaFile?filters=ContentType==image/webp",
            CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "MF_037 - Lấy danh sách file với sorting theo FileSize giảm dần")]
    public async Task GetFilesList_SortByFileSizeDesc_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var files = new List<MediaFileEntity>
        {
            new()
            {
                StorageType = "local",
                StoragePath = "sort-1.webp",
                ContentType = "image/webp",
                FileSize = 100000,
                DeletedAt = null
            },
            new()
            {
                StorageType = "local",
                StoragePath = "sort-2.webp",
                ContentType = "image/webp",
                FileSize = 200000,
                DeletedAt = null
            },
            new()
            {
                StorageType = "local",
                StoragePath = "sort-3.webp",
                ContentType = "image/webp",
                FileSize = 150000,
                DeletedAt = null
            },
            new()
            {
                StorageType = "local",
                StoragePath = "sort-4.webp",
                ContentType = "image/webp",
                FileSize = 50000,
                DeletedAt = null
            },
            new()
            {
                StorageType = "local",
                StoragePath = "sort-5.webp",
                ContentType = "image/webp",
                FileSize = 300000,
                DeletedAt = null
            }
        };
        db.MediaFiles.AddRange(files);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/MediaFile?sorts=-FileSize", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "MF_038 - Lấy danh sách file đã xoá thành công")]
    public async Task GetDeletedFilesList_WithPagination_Success()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var email = $"user_{uniqueId}@gmail.com";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Products.View], email);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var deletedFiles = new List<MediaFileEntity>();
        for(int i = 1; i <= 15; i++)
        {
            deletedFiles.Add(
                new MediaFileEntity
                {
                    StorageType = "local",
                    StoragePath = $"deleted-file-{i}.webp",
                    ContentType = "image/webp",
                    FileSize = 100000,
                    DeletedAt = DateTimeOffset.UtcNow.AddDays(-i)
                });
        }
        db.MediaFiles.AddRange(deletedFiles);
        await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);

        var response = await _client.GetAsync("/api/v1/MediaFile/deleted?page=1&pageSize=10", CancellationToken.None)
            .ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
#pragma warning restore CRR0035
}
