using System.Net;
using System.Net.Http.Json;
using Application.ApiContracts.Supplier.Responses;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Domain.Entities;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.DBContexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Suppliers.RemoveRange(db.Suppliers);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var suppliers = new List<Domain.Entities.Supplier>();
            for (int i = 1; i <= 25; i++)
            {
                suppliers.Add(new Domain.Entities.Supplier
                {
                    Name = $"Supplier {i}",
                    Phone = $"012345678{i:00}",
                    Address = $"Address {i}",
                    StatusId = i % 2 == 0 ? "active" : "inactive"
                });
            }
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Supplier?Page=1&PageSize=10", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(10);
        content.TotalCount.Should().Be(25);
        content.TotalPages.Should().Be(3);
        content.PageNumber.Should().Be(1);
    }

    [Fact(DisplayName = "SUP_032 - Lấy danh sách Supplier với phân trang tùy chỉnh")]
    public async Task GetSuppliers_CustomPagination_ReturnsPagedResult()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Suppliers.RemoveRange(db.Suppliers);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var suppliers = new List<Domain.Entities.Supplier>();
            for (int i = 1; i <= 25; i++)
            {
                suppliers.Add(new Domain.Entities.Supplier
                {
                    Name = $"Supplier {i}",
                    Phone = $"012345678{i:00}",
                    Address = $"Address {i}",
                    StatusId = "active"
                });
            }
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Supplier?Page=2&PageSize=5", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(5);
        content.TotalCount.Should().Be(25);
        content.TotalPages.Should().Be(5);
        content.PageNumber.Should().Be(2);
    }

    [Fact(DisplayName = "SUP_033 - Lấy danh sách Supplier với lọc theo Name")]
    public async Task GetSuppliers_FilterByName_ReturnsFilteredResult()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Suppliers.RemoveRange(db.Suppliers);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Test Supplier 1", Phone = "0111111111", Address = "Address 1", StatusId = "active" },
                new() { Name = "Test Supplier 2", Phone = "0222222222", Address = "Address 2", StatusId = "active" },
                new() { Name = "Other Supplier", Phone = "0333333333", Address = "Address 3", StatusId = "active" }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Supplier?Filters=Name@=*Test*", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(2);
        content.Items.Should().OnlyContain(s => s.Name!.Contains("Test"));
    }

    [Fact(DisplayName = "SUP_034 - Lấy danh sách Supplier với sắp xếp theo Name tăng dần")]
    public async Task GetSuppliers_SortByNameAscending_ReturnsSortedResult()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Suppliers.RemoveRange(db.Suppliers);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Zebra Supplier", Phone = "0111111111", Address = "Address", StatusId = "active" },
                new() { Name = "Alpha Supplier", Phone = "0222222222", Address = "Address", StatusId = "active" },
                new() { Name = "Beta Supplier", Phone = "0333333333", Address = "Address", StatusId = "active" }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Supplier?Sorts=Name", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content.Items?.First().Name.Should().Be("Alpha Supplier");
        content.Items?.Last().Name.Should().Be("Zebra Supplier");
    }

    [Fact(DisplayName = "SUP_035 - Lấy danh sách Supplier chỉ bao gồm trạng thái active và inactive")]
    public async Task GetSuppliers_OnlyActiveAndInactive_ExcludesDeleted()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Suppliers.RemoveRange(db.Suppliers);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Active 1", Phone = "0111111111", Address = "A", StatusId = "active", DeletedAt = null },
                new() { Name = "Inactive 1", Phone = "0222222222", Address = "A", StatusId = "inactive", DeletedAt = null },
                new() { Name = "Deleted 1", Phone = "0333333333", Address = "A", StatusId = "active", DeletedAt = DateTime.UtcNow }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Supplier", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.TotalCount.Should().Be(2);
        content.Items.Should().NotContain(s => string.Compare(s.Name, "Deleted 1") == 0);
    }

    [Fact(DisplayName = "SUP_036 - Lấy danh sách Supplier đã xóa với phân trang")]
    public async Task GetDeletedSuppliers_WithPagination_ReturnsDeletedOnly()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            db.Suppliers.RemoveRange(db.Suppliers);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Active 1", Phone = "0111111111", Address = "A", StatusId = "active", DeletedAt = null },
                new() { Name = "Deleted 1", Phone = "0222222222", Address = "A", StatusId = "active", DeletedAt = DateTime.UtcNow },
                new() { Name = "Deleted 2", Phone = "0333333333", Address = "A", StatusId = "inactive", DeletedAt = DateTime.UtcNow }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync("/api/v1/Supplier/deleted?Page=1&PageSize=10", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PagedResult<SupplierResponse>>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.TotalCount.Should().Be(2);
        content.Items.Should().NotContain(s => string.Compare(s.Name, "Active 1") == 0);
    }

    [Fact(DisplayName = "SUP_037 - Lấy chi tiết Supplier thành công với đầy đủ thông tin")]
    public async Task GetSupplierById_WithFullInfo_ReturnsCompleteSupplier()
    {
        // Arrange
        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var supplier = new Domain.Entities.Supplier
            {
                Name = "Full Info Supplier",
                Phone = "0123456789",
                Email = "test@test.com",
                Address = "Full Address",
                TaxIdentificationNumber = "1234567890",
                StatusId = "active",
                Notes = "Test notes"
            };
            await db.Suppliers.AddAsync(supplier, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
            supplierId = supplier.Id;
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.Id.Should().Be(supplierId);
        content.Name.Should().Be("Full Info Supplier");
        content.Phone.Should().Be("0123456789");
        content.Email.Should().Be("test@test.com");
        content.TaxIdentificationNumber.Should().Be("1234567890");
    }

    [Fact(DisplayName = "SUP_038 - Lấy chi tiết Supplier thất bại khi Supplier đã bị xóa")]
    public async Task GetSupplierById_DeletedSupplier_ReturnsNotFound()
    {
        // Arrange
        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var supplier = new Domain.Entities.Supplier
            {
                Name = "Deleted Supplier",
                Phone = "0123456789",
                Address = "Address",
                StatusId = "active",
                DeletedAt = DateTime.UtcNow
            };
            await db.Suppliers.AddAsync(supplier, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
            supplierId = supplier.Id;
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}").ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SUP_039 - Lấy chi tiết Supplier thất bại khi Id không tồn tại")]
    public async Task GetSupplierById_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Supplier/999999", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "SUP_040 - Xóa nhiều Supplier thành công")]
    public async Task DeleteManySuppliers_AllValid_SuccessfullyDeletes()
    {
        // Arrange
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Supplier 1", Phone = "0111111111", Address = "A", StatusId = "active" },
                new() { Name = "Supplier 2", Phone = "0222222222", Address = "A", StatusId = "active" },
                new() { Name = "Supplier 3", Phone = "0333333333", Address = "A", StatusId = "active" }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
            supplierIds = [.. suppliers.Select(s => s.Id)];
        }

        var request = new DeleteManySuppliersCommand { Ids = supplierIds };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Supplier/delete-many", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var deletedSuppliers = db.Suppliers
                .Where(s => supplierIds.Contains(s.Id))
                .ToList();
            deletedSuppliers.Should().AllSatisfy(s => s.DeletedAt.Should().NotBeNull());
        }
    }

    [Fact(DisplayName = "SUP_041 - Xóa nhiều Supplier thất bại khi một trong số đó còn Input Receipt Working")]
    public async Task DeleteManySuppliers_OneHasWorkingReceipt_FailsForAll()
    {
        // Arrange
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var supplier1 = new Domain.Entities.Supplier { Name = "Supplier 1", Phone = "0111111111", Address = "A", StatusId = "active" };
            var supplier2 = new Domain.Entities.Supplier { Name = "Supplier 2", Phone = "0222222222", Address = "A", StatusId = "active" };
            await db.Suppliers.AddRangeAsync([supplier1, supplier2], CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            var input = new Input { SupplierId = supplier2.Id, StatusId = "working" };
            await db.InputReceipts.AddAsync(input, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            supplierIds = [supplier1.Id, supplier2.Id];
        }

        var request = new DeleteManySuppliersCommand { Ids = supplierIds };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Supplier/delete-many", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
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
        // Arrange
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Supplier 1", Phone = "0111111111", Address = "A", StatusId = "active", DeletedAt = DateTime.UtcNow },
                new() { Name = "Supplier 2", Phone = "0222222222", Address = "A", StatusId = "inactive", DeletedAt = DateTime.UtcNow }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
            supplierIds = [.. suppliers.Select(s => s.Id)];
        }

        var request = new RestoreManySuppliersCommand { Ids = supplierIds };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/Supplier/restore-many", request).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var restoredSuppliers = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            restoredSuppliers.Should().AllSatisfy(s => s.DeletedAt.Should().BeNull());
        }
    }

    [Fact(DisplayName = "SUP_043 - Cập nhật trạng thái nhiều Supplier thành công")]
    public async Task UpdateManySupplierStatus_ValidStatus_SuccessfullyUpdates()
    {
        // Arrange
        List<int> supplierIds;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var suppliers = new List<Domain.Entities.Supplier>
            {
                new() { Name = "Supplier 1", Phone = "0111111111", Address = "A", StatusId = "active" },
                new() { Name = "Supplier 2", Phone = "0222222222", Address = "A", StatusId = "active" }
            };
            await db.Suppliers.AddRangeAsync(suppliers, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
            supplierIds = [.. suppliers.Select(s => s.Id)];
        }

        var request = new UpdateManySupplierStatusCommand
        { 
            Ids = supplierIds,
            StatusId = "inactive"
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/v1/Supplier/update-status-many", request, CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var updatedSuppliers = db.Suppliers.Where(s => supplierIds.Contains(s.Id)).ToList();
            updatedSuppliers.Should().AllSatisfy(s => s.StatusId.Should().Be("inactive"));
        }
    }

    [Fact(DisplayName = "SUP_044 - Tính toán TotalInputValue chính xác với nhiều Input Receipt")]
    public async Task GetSupplierById_MultipleInputReceipts_CalculatesCorrectTotal()
    {
        // Arrange
        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var supplier = new Domain.Entities.Supplier
            {
                Name = "Supplier With Inputs",
                Phone = "0123456789",
                Address = "Address",
                StatusId = "active"
            };
            await db.Suppliers.AddAsync(supplier, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);
            supplierId = supplier.Id;

            var inputs = new List<Input>
            {
                new() { SupplierId = supplierId, StatusId = "completed" },
                new() { SupplierId = supplierId, StatusId = "completed" },
                new() { SupplierId = supplierId, StatusId = "completed" }
            };
            await db.InputReceipts.AddRangeAsync(inputs, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            // Add InputInfo records to calculate total
            var inputInfos = new List<InputInfo>
            {
                new() { InputId = inputs[0].Id, InputPrice = 1000.00m, Count = 10 }, // 10000
                new() { InputId = inputs[1].Id, InputPrice = 2000.00m, Count = 20 }, // 40000
                new() { InputId = inputs[2].Id, InputPrice = 500.00m, Count = 30 }   // 15000
            };
            await db.InputInfos.AddRangeAsync(inputInfos, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.TotalInput.Should().Be(65000); // 10000 + 40000 + 15000
    }

    [Fact(DisplayName = "SUP_045 - Tính toán TotalInputValue không bao gồm Input Receipt ở trạng thái khác completed")]
    public async Task GetSupplierById_MixedInputStatuses_OnlyCountsCompleted()
    {
        // Arrange
        int supplierId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var supplier = new Domain.Entities.Supplier
            {
                Name = "Supplier Mixed Inputs",
                Phone = "0123456789",
                Address = "Address",
                StatusId = "active"
            };
            await db.Suppliers.AddAsync(supplier, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
            supplierId = supplier.Id;

            var inputs = new List<Input>
            {
                new() { SupplierId = supplierId, StatusId = "completed" },
                new() { SupplierId = supplierId, StatusId = "working" },
                new() { SupplierId = supplierId, StatusId = "cancelled" }
            };
            await db.InputReceipts.AddRangeAsync(inputs, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;

            // Add InputInfo records - only completed should be counted
            var inputInfos = new List<InputInfo>
            {
                new() { InputId = inputs[0].Id, InputPrice = 100m, Count = 100 }, // 10000 - completed
                new() { InputId = inputs[1].Id, InputPrice = 200m, Count = 50 },  // 10000 - working (not counted)
                new() { InputId = inputs[2].Id, InputPrice = 300m, Count = 100 }  // 30000 - cancelled (not counted)
            };
            await db.InputInfos.AddRangeAsync(inputInfos, CancellationToken.None).ConfigureAwait(true);
            await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(true);;
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/Supplier/{supplierId}", CancellationToken.None).ConfigureAwait(true);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierResponse>(CancellationToken.None).ConfigureAwait(true);
        content.Should().NotBeNull();
        content!.TotalInput.Should().Be(10000); // Only completed input
    }
#pragma warning restore CRR0035
}
