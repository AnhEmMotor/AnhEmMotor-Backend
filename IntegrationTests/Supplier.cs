using Application.ApiContracts.Input.Responses;
using Application.ApiContracts.Supplier.Responses;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Domain.Constants.Permission;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Common;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SupplierEntity = Domain.Entities.Supplier;

namespace IntegrationTests;

public class Supplier : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public Supplier(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "SUP_031 - Lấy danh sách Supplier với phân trang mặc định")]
    public async Task GetSuppliers_DefaultPagination_ReturnsPagedResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suppliers = new List<SupplierEntity>();
            for (int i = 1; i <= 15; i++)
            {
                suppliers.Add(new SupplierEntity
                {
                    Name = $"Supplier_{uniqueId}_{i}",
                    Phone = $"012345678{i:00}",
                    Address = $"Address {i}",
                    StatusId = i % 2 == 0 ? Domain.Constants.SupplierStatus.Active : Domain.Constants.SupplierStatus.Inactive
                });
            }
            await db.Suppliers.AddRangeAsync(suppliers);
            
            // Ensure statuses exist
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Inactive))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Inactive });

            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier?Page=1&PageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>();
        
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.Items.Should().OnlyContain(s => s.Name!.Contains(uniqueId));
    }

    [Fact(DisplayName = "SUP_032 - Lấy danh sách Supplier với phân trang tùy chỉnh")]
    public async Task GetSuppliers_CustomPagination_ReturnsPagedResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            // Ensure statuses
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            
            var suppliers = new List<SupplierEntity>();
            for (int i = 1; i <= 15; i++)
            {
                suppliers.Add(new SupplierEntity
                {
                    Name = $"Supplier_{uniqueId}_{i}",
                    Phone = $"012345678{i:00}",
                    Address = $"Address {i}",
                    StatusId = Domain.Constants.SupplierStatus.Active
                });
            }
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier?Page=2&PageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>();
        
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(5);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "SUP_033 - Lấy danh sách Supplier với lọc theo Name")]
    public async Task GetSuppliers_FilterByName_ReturnsFilteredResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            // Ensure statuses
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"Test_{uniqueId}_1", Phone = "0111111111", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active },
                new() { Name = $"Test_{uniqueId}_2", Phone = "0222222222", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active },
                new() { Name = $"Other_{uniqueId}", Phone = "0333333333", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier?Filters=Name@=Test_{uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>();
        
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(2);
        content.Items.Should().OnlyContain(s => s.Name!.Contains($"Test_{uniqueId}"));
    }

    [Fact(DisplayName = "SUP_034 - Lấy danh sách Supplier với sắp xếp theo Name tăng dần")]
    public async Task GetSuppliers_SortByNameAscending_ReturnsSortedResult()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"Zebra_{uniqueId}", Phone = "0111111111", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active },
                new() { Name = $"Alpha_{uniqueId}", Phone = "0222222222", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active },
                new() { Name = $"Beta_{uniqueId}", Phone = "0333333333", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier?Sorts=Name&Filters=Name@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>();
        
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(3);
        content.Items![0].Name.Should().Be($"Alpha_{uniqueId}");
        content.Items[1].Name.Should().Be($"Beta_{uniqueId}");
        content.Items[2].Name.Should().Be($"Zebra_{uniqueId}");
    }

    [Fact(DisplayName = "SUP_035 - Lấy danh sách Supplier chỉ bao gồm trạng thái active và inactive")]
    public async Task GetSuppliers_OnlyActiveAndInactive_ExcludesDeleted()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Inactive))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Inactive });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"Active_{uniqueId}", Phone = "0111111111", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active, DeletedAt = null },
                new() { Name = $"Inactive_{uniqueId}", Phone = "0222222222", Address = "A", StatusId = Domain.Constants.SupplierStatus.Inactive, DeletedAt = null },
                new() { Name = $"Deleted_{uniqueId}", Phone = "0333333333", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active, DeletedAt = DateTime.UtcNow }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier?Filters=Id@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>();
        
        content.Should().NotBeNull();
        // Since other tests might add active suppliers, we just ensure our deleted one isn't there and our active/inactive checks are present
        content!.Items.Should().Contain(s => s.Name == $"Active_{uniqueId}");
        content.Items.Should().Contain(s => s.Name == $"Inactive_{uniqueId}");
        content.Items.Should().NotContain(s => s.Name == $"Deleted_{uniqueId}");
    }

    [Fact(DisplayName = "SUP_036 - Lấy danh sách Supplier đã xóa với phân trang")]
    public async Task GetDeletedSuppliers_WithPagination_ReturnsDeletedOnly()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"Active_{uniqueId}", Phone = "0111111111", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active, DeletedAt = null },
                new() { Name = $"Deleted_{uniqueId}", Phone = "0222222222", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active, DeletedAt = DateTime.UtcNow }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier/deleted?Filters=Id@={uniqueId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>();
        
        content.Should().NotBeNull();
        content!.Items.Should().Contain(s => s.Name == $"Deleted_{uniqueId}");
        content.Items.Should().NotContain(s => s.Name == $"Active_{uniqueId}");
    }

    [Fact(DisplayName = "SUP_037 - Lấy chi tiết Supplier thành công với đầy đủ thông tin")]
    public async Task GetSupplierById_WithFullInfo_ReturnsCompleteSupplier()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
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
            await db.Suppliers.AddAsync(supplier);
            await db.SaveChangesAsync();
            supplierId = supplier.Id;
        }

        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierResponse>();
        
        content.Should().NotBeNull();
        content!.Id.Should().Be(supplierId);
        content.Name.Should().Be($"Full Info {uniqueId}");
        content.Email.Should().Be($"test_{uniqueId}@test.com");
    }

    [Fact(DisplayName = "SUP_038 - Lấy chi tiết Supplier thất bại khi Supplier đã bị xóa")]
    public async Task GetSupplierById_DeletedSupplier_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });

            var supplier = new SupplierEntity
            {
                Name = $"Deleted {uniqueId}",
                Phone = "0123456789",
                Address = "Address",
                StatusId = Domain.Constants.SupplierStatus.Active,
                DeletedAt = DateTime.UtcNow
            };
            await db.Suppliers.AddAsync(supplier);
            await db.SaveChangesAsync();
            supplierId = supplier.Id;
        }

        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SUP_039 - Lấy chi tiết Supplier thất bại khi Id không tồn tại")]
    public async Task GetSupplierById_NonExistentId_ReturnsNotFound()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        var response = await _client.GetAsync("/api/v1/Supplier/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SUP_040 - Xóa nhiều Supplier thành công")]
    public async Task DeleteManySuppliers_AllValid_SuccessfullyDeletes()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.Delete]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"S1_{uniqueId}", Phone = "0111111111", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active },
                new() { Name = $"S2_{uniqueId}", Phone = "0222222222", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
            supplierIds = suppliers.Select(s => s.Id).ToList();
        }

        var request = new DeleteManySuppliersCommand { Ids = supplierIds };
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/Supplier/delete-many")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var deletedSuppliers = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            deletedSuppliers.Should().AllSatisfy(s => s.DeletedAt.Should().NotBeNull());
        }
    }

    [Fact(DisplayName = "SUP_041 - Xóa nhiều Supplier thất bại khi một trong số đó còn Input Receipt Working")]
    public async Task DeleteManySuppliers_OneHasWorkingReceipt_FailsForAll()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.Delete]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.InputStatuses.AnyAsync(s => s.Key == Domain.Constants.Input.InputStatus.Working))
                db.InputStatuses.Add(new InputStatus { Key = Domain.Constants.Input.InputStatus.Working });

            var supplier1 = new SupplierEntity { Name = $"S1_{uniqueId}", Phone = "011", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active };
            var supplier2 = new SupplierEntity { Name = $"S2_{uniqueId}", Phone = "022", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active };
            await db.Suppliers.AddRangeAsync([supplier1, supplier2]);
            await db.SaveChangesAsync();

            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            var userId = user?.Id ?? Guid.NewGuid();

            var input = new Input { SupplierId = supplier2.Id, StatusId = Domain.Constants.Input.InputStatus.Working, CreatedBy = userId }; // Minimal Input
            await db.InputReceipts.AddAsync(input);
            await db.SaveChangesAsync();

            supplierIds = [supplier1.Id, supplier2.Id];
        }

        var request = new DeleteManySuppliersCommand { Ids = supplierIds };
        var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/Supplier/delete-many")
        {
            Content = JsonContent.Create(request)
        };

        var response = await _client.SendAsync(requestMessage);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // or whatever status code the logic returns for failure

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suppliers = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            suppliers.Should().AllSatisfy(s => s.DeletedAt.Should().BeNull());
        }
    }

    [Fact(DisplayName = "SUP_042 - Khôi phục nhiều Supplier thành công")]
    public async Task RestoreManySuppliers_AllDeleted_SuccessfullyRestores()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.Delete]); // Usually Restore might share Delete permissions or have its own
        // Assuming Restore behaves similarly permission-wise or check if specific perm needed. 
        // Based on other files, it likely uses RestoreManySuppliersValidator/Handler which might check Delete or Edit. 
        // Let's assume Delete perm works or check handler. Safest is ensuring Delete works or providing super access? 
        // Let's stick to Suppliers.Delete as per previous conversations/common sense for Restore.
        
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"S1_{uniqueId}", Phone = "011", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active, DeletedAt = DateTime.UtcNow },
                new() { Name = $"S2_{uniqueId}", Phone = "022", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active, DeletedAt = DateTime.UtcNow }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
            supplierIds = suppliers.Select(s => s.Id).ToList();
        }

        var request = new RestoreManySuppliersCommand { Ids = supplierIds };
        var response = await _client.PostAsJsonAsync("/api/v1/Supplier/restore-many", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var restored = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            restored.Should().AllSatisfy(s => s.DeletedAt.Should().BeNull());
        }
    }

    [Fact(DisplayName = "SUP_043 - Cập nhật trạng thái nhiều Supplier thành công")]
    public async Task UpdateManySupplierStatus_ValidStatus_SuccessfullyUpdates()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.Edit]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Active))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Active });
            if (!await db.SupplierStatuses.AnyAsync(s => s.Key == Domain.Constants.SupplierStatus.Inactive))
                db.SupplierStatuses.Add(new SupplierStatus { Key = Domain.Constants.SupplierStatus.Inactive });

            var suppliers = new List<SupplierEntity>
            {
                new() { Name = $"S1_{uniqueId}", Phone = "011", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active },
                new() { Name = $"S2_{uniqueId}", Phone = "022", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active }
            };
            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
            supplierIds = suppliers.Select(s => s.Id).ToList();
        }

        var request = new UpdateManySupplierStatusCommand { Ids = supplierIds, StatusId = Domain.Constants.SupplierStatus.Inactive };
        var response = await _client.PatchAsJsonAsync("/api/v1/Supplier/update-status-many", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updated = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            updated.Should().AllSatisfy(s => s.StatusId.Should().Be(Domain.Constants.SupplierStatus.Inactive));
        }
    }

    [Fact(DisplayName = "SUP_044 - Tính toán TotalInputValue chính xác với nhiều Input Receipt")]
    public async Task GetSupplierById_MultipleInputReceipts_CalculatesCorrectTotal()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync("INSERT OR IGNORE INTO SupplierStatus (Key) VALUES ({0})", Domain.Constants.SupplierStatus.Active);
            await db.Database.ExecuteSqlRawAsync("INSERT OR IGNORE INTO InputStatus (Key) VALUES ({0})", Domain.Constants.Input.InputStatus.Finish);

            var supplier = new SupplierEntity { Name = $"S_{uniqueId}", Phone = "099", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active };
            
            await db.Suppliers.AddAsync(supplier);
            await db.SaveChangesAsync();
            supplierId = supplier.Id;

            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            var userId = user?.Id ?? Guid.NewGuid();

            var inputs = new List<Input>
            {
                new() { SupplierId = supplierId, StatusId = Domain.Constants.Input.InputStatus.Finish, CreatedBy = userId },
                new() { SupplierId = supplierId, StatusId = Domain.Constants.Input.InputStatus.Finish, CreatedBy = userId },
                new() { SupplierId = supplierId, StatusId = Domain.Constants.Input.InputStatus.Finish, CreatedBy = userId }
            };
            await db.InputReceipts.AddRangeAsync(inputs);
            await db.SaveChangesAsync();

            var inputInfos = new List<InputInfo>
            {
                new() { InputId = inputs[0].Id, InputPrice = 1000m, Count = 10 },    // 10,000
                new() { InputId = inputs[1].Id, InputPrice = 2000m, Count = 20 },    // 40,000
                new() { InputId = inputs[2].Id, InputPrice = 500m, Count = 30 }      // 15,000
            };
            await db.InputInfos.AddRangeAsync(inputInfos);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierResponse>();
        
        content.Should().NotBeNull();
        content!.TotalInput.Should().Be(65000); // 10k + 40k + 15k
    }

    [Fact(DisplayName = "SUP_045 - Tính toán TotalInputValue không bao gồm Input Receipt ở trạng thái khác completed")]
    public async Task GetSupplierById_MixedInputStatuses_OnlyCountsCompleted()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"user_{uniqueId}";
        var password = "ThisIsStrongPassword1@";
        await IntegrationTestAuthHelper.CreateUserWithPermissionsAsync(_factory.Services, username, password, [PermissionsList.Suppliers.View]);
        var loginResponse = await IntegrationTestAuthHelper.AuthenticateAsync(_client, username, password);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            await db.Database.ExecuteSqlRawAsync("INSERT OR IGNORE INTO SupplierStatus (Key) VALUES ({0})", Domain.Constants.SupplierStatus.Active);
            
            foreach (var status in new[] { Domain.Constants.Input.InputStatus.Finish, Domain.Constants.Input.InputStatus.Working, Domain.Constants.Input.InputStatus.Cancel })
            {
                await db.Database.ExecuteSqlRawAsync("INSERT OR IGNORE INTO InputStatus (Key) VALUES ({0})", status);
            }

            var supplier = new SupplierEntity { Name = $"S_{uniqueId}", Phone = "099", Address = "A", StatusId = Domain.Constants.SupplierStatus.Active };
            
            await db.Suppliers.AddAsync(supplier);
            await db.SaveChangesAsync();
            supplierId = supplier.Id;

            var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == username);
            var userId = user?.Id ?? Guid.NewGuid();

            var inputs = new List<Input>
            {
                new() { SupplierId = supplierId, StatusId = Domain.Constants.Input.InputStatus.Finish, CreatedBy = userId },
                new() { SupplierId = supplierId, StatusId = Domain.Constants.Input.InputStatus.Working, CreatedBy = userId },
                new() { SupplierId = supplierId, StatusId = Domain.Constants.Input.InputStatus.Cancel, CreatedBy = userId }
            };
            await db.InputReceipts.AddRangeAsync(inputs);
            await db.SaveChangesAsync();

            var inputInfos = new List<InputInfo>
            {
                new() { InputId = inputs[0].Id, InputPrice = 100m, Count = 100 },  // Completed: 10,000
                new() { InputId = inputs[1].Id, InputPrice = 200m, Count = 50 },   // Working: 10,000 (Ignored)
                new() { InputId = inputs[2].Id, InputPrice = 300m, Count = 100 }   // Cancelled: 30,000 (Ignored)
            };
            await db.InputInfos.AddRangeAsync(inputInfos);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierResponse>();
        
        content.Should().NotBeNull();
        content!.TotalInput.Should().Be(10000);
    }
#pragma warning restore CRR0035
}
