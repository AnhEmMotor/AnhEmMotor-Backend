using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Domain.Constants;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IntegrationTests;

[Collection("Shared Integration Collection")]
public class UserAvatar : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public UserAvatar(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "USER_071 - Người dùng tự tải lên ảnh đại diện thành công")]
    public async Task UploadAvatar_Self_Success()
    {
        // 1. Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "StrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var content = new MultipartFormDataContent();
        byte[] gifData = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x21, 0xF9, 0x04, 0x01, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x01, 0x44, 0x00, 0x3B };
        var fileContent = new ByteArrayContent(gifData);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/gif");
        content.Add(fileContent, "file", "avatar.gif");

        // 2. Act
        var response = await _client.PostAsync("/api/v1/User/avatar", content).ConfigureAwait(true);

        // 3. Assert
        if(response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            throw new Exception($"Upload failed with status {response.StatusCode}. Content: {errorContent}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var avatarUrl = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        avatarUrl.Should().NotBeNullOrEmpty();
        avatarUrl.Should().Contain("avatars/");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username).ConfigureAwait(true);
        user!.AvatarUrl.Should().Be(avatarUrl.Trim('\"'));
    }

    [Fact(DisplayName = "USER_072 - Admin tải lên ảnh đại diện cho người dùng khác thành công")]
    public async Task UploadAvatar_AdminForUser_Success()
    {
        // 1. Arrange
        var adminUniqueId = Guid.NewGuid().ToString("N")[..8];
        var adminUsername = $"admin_{adminUniqueId}";
        var password = "StrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            adminUsername,
            password,
            [PermissionsList.Users.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, adminUsername, password, CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var targetUniqueId = Guid.NewGuid().ToString("N")[..8];
        var targetUser = await IntegrationTestAuthHelper.CreateUserAsync(_factory.Services, $"target_{targetUniqueId}", "Pass123!", CancellationToken.None)
            .ConfigureAwait(true);

        var content = new MultipartFormDataContent();
        byte[] gifData = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x21, 0xF9, 0x04, 0x01, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x01, 0x44, 0x00, 0x3B };
        var fileContent = new ByteArrayContent(gifData);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/gif");
        content.Add(fileContent, "file", "avatar.gif");

        // 2. Act
        var response = await _client.PostAsync($"/api/v1/UserManager/{targetUser.Id}/avatar", content).ConfigureAwait(true);

        // 3. Assert
        if(response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            throw new Exception($"Admin upload failed with status {response.StatusCode}. Content: {errorContent}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var avatarUrl = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        avatarUrl.Should().NotBeNullOrEmpty();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var updatedUser = await db.Users.FindAsync(targetUser.Id).ConfigureAwait(true);
        updatedUser!.AvatarUrl.Should().Be(avatarUrl.Trim('\"'));
    }

    [Fact(DisplayName = "PERM_INT_016 - Lấy cấu trúc quyền hạn thành công")]
    public async Task GetPermissionStructure_Success()
    {
        // 1. Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"admin_{uniqueId}";
        var password = "StrongPassword1@";

        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [PermissionsList.Roles.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password, CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        // 2. Act
        var response = await _client.GetAsync("/api/v1/Permission/structure").ConfigureAwait(true);

        // 3. Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var structure = await response.Content.ReadFromJsonAsync<Application.ApiContracts.Permission.Responses.PermissionStructureResponse>().ConfigureAwait(true);
        
        structure.Should().NotBeNull();
        structure!.Groups.Should().NotBeEmpty();
        structure.Groups.Should().ContainKey("Sản phẩm");
        structure.Dependencies.Should().ContainKey(PermissionsList.Products.Create);
    }
}
