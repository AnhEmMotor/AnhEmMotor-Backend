using Application.ApiContracts.Supplier.Responses;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Domain.Constants.Permission.Permissions;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SupplierEntity = Domain.Entities.Supplier;

namespace IntegrationTests;

using System;
using System.Threading.Tasks;
using Xunit;

public class Supplier : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Supplier(IntegrationTestWebAppFactory factory)
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

    #pragma warning disable IDE0079
    #pragma warning disable CRR0035
    [Fact(DisplayName = "SUP_031 - L?y danh sách Supplier v?i phân trang m?c d?nh")]
    public async Task GetSuppliers_DefaultPagination_ReturnsPagedResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suppliers = new List<SupplierEntity>();
            for (int i = 1; i <= 15; i++)
            {
                suppliers.Add(
                    new SupplierEntity
                    {
                        Name = $"Supplier_{uniqueId}_{i}",
                        Phone = $"012345678{i:00}",
                        Address = $"Address {i}",
                        StatusId =
                            i % 2 == 0
                                    ? Domain.Constants.SupplierStatus.Active
                                    : Domain.Constants.SupplierStatus.Inactive
                    });
            }
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Inactive) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Inactive });
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync($"/api/v1/Supplier?Page=1&PageSize=10", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.Items.Should().OnlyContain(s => s.Name!.Contains(uniqueId));
    }

    [Fact(DisplayName = "SUP_032 - L?y danh sách Supplier v?i phân trang tùy ch?nh")]
    public async Task GetSuppliers_CustomPagination_ReturnsPagedResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var suppliers = new List<SupplierEntity>();
            for (int i = 1; i <= 15; i++)
            {
                suppliers.Add(
                    new SupplierEntity
                    {
                        Name = $"Supplier_{uniqueId}_{i}",
                        Phone = $"012345678{i:00}",
                        Address = $"Address {i}",
                        StatusId = Domain.Constants.SupplierStatus.Active
                    });
            }
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync($"/api/v1/Supplier?Page=2&PageSize=5", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(5);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "SUP_033 - L?y danh sách Supplier v?i l?c theo Name")]
    public async Task GetSuppliers_FilterByName_ReturnsFilteredResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"Test_{uniqueId}_1",
                    Phone = "0111111111",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                },
                new()
                {
                    Name = $"Test_{uniqueId}_2",
                    Phone = "0222222222",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                },
                new()
                {
                    Name = $"Other_{uniqueId}",
                    Phone = "0333333333",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync($"/api/v1/Supplier?Filters=Name@=Test_{uniqueId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(2);
        content.Items.Should().OnlyContain(s => s.Name!.Contains($"Test_{uniqueId}"));
    }

    [Fact(DisplayName = "SUP_034 - L?y danh sách Supplier v?i s?p x?p theo Name tang d?n")]
    public async Task GetSuppliers_SortByNameAscending_ReturnsSortedResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"Zebra_{uniqueId}",
                    Phone = "0111111111",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                },
                new()
                {
                    Name = $"Alpha_{uniqueId}",
                    Phone = "0222222222",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                },
                new()
                {
                    Name = $"Beta_{uniqueId}",
                    Phone = "0333333333",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/Supplier?Sorts=Name&Filters=Name@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items![0].Name.Should().Be($"Alpha_{uniqueId}");
        content.Items[1].Name.Should().Be($"Beta_{uniqueId}");
        content.Items[2].Name.Should().Be($"Zebra_{uniqueId}");
    }

    [Fact(DisplayName = "SUP_035 - L?y danh sách Supplier ch? bao g?m tr?ng thái active và inactive")]
    public async Task GetSuppliers_OnlyActiveAndInactive_ExcludesDeleted()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Inactive) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Inactive });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"Active_{uniqueId}",
                    Phone = "0111111111",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active,
                    DeletedAt = null
                },
                new()
                {
                    Name = $"Inactive_{uniqueId}",
                    Phone = "0222222222",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Inactive,
                    DeletedAt = null
                },
                new()
                {
                    Name = $"Deleted_{uniqueId}",
                    Phone = "0333333333",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active,
                    DeletedAt = DateTime.UtcNow
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync($"/api/v1/Supplier?Filters=Id@={uniqueId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(s => string.Compare(s.Name, $"Active_{uniqueId}") == 0);
        content.Items.Should().Contain(s => string.Compare(s.Name, $"Inactive_{uniqueId}") == 0);
        content.Items.Should().NotContain(s => string.Compare(s.Name, $"Deleted_{uniqueId}") == 0);
    }

    [Fact(DisplayName = "SUP_036 - L?y danh sách Supplier dã xóa v?i phân trang")]
    public async Task GetDeletedSuppliers_WithPagination_ReturnsDeletedOnly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"Active_{uniqueId}",
                    Phone = "0111111111",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active,
                    DeletedAt = null
                },
                new()
                {
                    Name = $"Deleted_{uniqueId}",
                    Phone = "0222222222",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active,
                    DeletedAt = DateTime.UtcNow
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
        }
        var response = await _client.GetAsync(
            $"/api/v1/Supplier/deleted?Filters=Id@={uniqueId}",
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Items.Should().Contain(s => string.Compare(s.Name, $"Deleted_{uniqueId}") == 0);
        content.Items.Should().NotContain(s => string.Compare(s.Name, $"Active_{uniqueId}") == 0);
    }

    [Fact(DisplayName = "SUP_037 - L?y chi ti?t Supplier thành công v?i d?y d? thông tin")]
    public async Task GetSupplierById_WithFullInfo_ReturnsCompleteSupplier()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var supplier = new SupplierEntity
            {
                Name = $"Full Info {uniqueId}",
                Phone = "0123456789",
                Email = $"test_{uniqueId}@test.com",
                Address = "Full Address",
                TaxIdentificationNumber = "1234567890",
                StatusId = Domain.Constants.SupplierStatus.Active,
                Notes = "Test notes"
            };
            await db.Suppliers.AddAsync(supplier, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            supplierId = supplier.Id;
        }
        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response!.Content
            .ReadFromJsonAsync<SupplierResponse>(CancellationToken.None)
            .ConfigureAwait(true);
        content!.Should().NotBeNull();
        content!.Id.Should().Be(supplierId);
        content.Name.Should().Be($"Full Info {uniqueId}");
        content.Email.Should().Be($"test_{uniqueId}@test.com");
    }

    [Fact(DisplayName = "SUP_038 - L?y chi ti?t Supplier th?t b?i khi Supplier dã b? xóa")]
    public async Task GetSupplierById_DeletedSupplier_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var supplier = new SupplierEntity
            {
                Name = $"Deleted {uniqueId}",
                Phone = "0123456789",
                Address = "Address",
                StatusId = Domain.Constants.SupplierStatus.Active,
                DeletedAt = DateTime.UtcNow
            };
            await db.Suppliers.AddAsync(supplier, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            supplierId = supplier.Id;
        }
        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}", CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SUP_039 - L?y chi ti?t Supplier th?t b?i khi Id không t?n t?i")]
    public async Task GetSupplierById_NonExistentId_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.View],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        var response = await _client.GetAsync("/api/v1/Supplier/999999", CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SUP_040 - Xóa nhi?u Supplier thành công")]
    public async Task DeleteManySuppliers_AllValid_SuccessfullyDeletes()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.Delete],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"S1_{uniqueId}",
                    Phone = "0111111111",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                },
                new()
                {
                    Name = $"S2_{uniqueId}",
                    Phone = "0222222222",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            supplierIds = [.. suppliers.Select(s => s.Id)];
        }
        var request = new DeleteManySuppliersCommand { Ids = supplierIds };
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/Supplier/delete-many")
        {
            Content = JsonContent.Create(request)
        };
        var response = await _client.SendAsync(requestMessage, CancellationToken.None).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var deletedSuppliers = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            deletedSuppliers.Should().AllSatisfy(s => s.DeletedAt.Should().NotBeNull());
        }
    }

    [Fact(DisplayName = "SUP_042 - Khôi ph?c nhi?u Supplier thành công")]
    public async Task RestoreManySuppliers_AllDeleted_SuccessfullyRestores()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.Delete],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"S1_{uniqueId}",
                    Phone = "011",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active,
                    DeletedAt = DateTime.UtcNow
                },
                new()
                {
                    Name = $"S2_{uniqueId}",
                    Phone = "022",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active,
                    DeletedAt = DateTime.UtcNow
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            supplierIds = [.. suppliers.Select(s => s.Id)];
        }
        var request = new RestoreManySuppliersCommand { Ids = supplierIds };
        var response = await _client.PostAsJsonAsync("/api/v1/Supplier/restore-many", request).ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var restored = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            restored.Should().AllSatisfy(s => s.DeletedAt.Should().BeNull());
        }
    }

    [Fact(DisplayName = "SUP_043 - C?p nh?t tr?ng thái nhi?u Supplier thành công")]
    public async Task UpdateManySupplierStatus_ValidStatus_SuccessfullyUpdates()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(
            _factory.Services,
            username,
            password,
            [Suppliers.Edit],
            CancellationToken.None)
            .ConfigureAwait(true);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(
            _client,
            username,
            password,
            CancellationToken.None)
            .ConfigureAwait(true);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Active) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.SupplierStatuses
                .AnyAsync(
                    s => string.Compare(s.Key, Domain.Constants.SupplierStatus.Inactive) == 0,
                    CancellationToken.None)
                .ConfigureAwait(true))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Inactive });
            var suppliers = new List<SupplierEntity>
            {
                new()
                {
                    Name = $"S1_{uniqueId}",
                    Phone = "011",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                },
                new()
                {
                    Name = $"S2_{uniqueId}",
                    Phone = "022",
                    Address = "A",
                    StatusId = Domain.Constants.SupplierStatus.Active
                }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            supplierIds = [.. suppliers.Select(s => s.Id)];
        }
        var request = new UpdateManySupplierStatusCommand
        {
            Ids = supplierIds,
            StatusId = Domain.Constants.SupplierStatus.Inactive
        };
        var response = await _client.PatchAsJsonAsync(
            "/api/v1/Supplier/update-status-many",
            request,
            CancellationToken.None)
            .ConfigureAwait(true);
        response!.StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updated = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            updated.Should().AllSatisfy(s => s.StatusId.Should().Be(Domain.Constants.SupplierStatus.Inactive));
        }
    }
    #pragma warning restore CRR0035
}

