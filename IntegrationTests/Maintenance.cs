using Domain.Entities;
using FluentAssertions;
using Infrastructure.DBContexts;
using IntegrationTests.SetupClass;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests;

public class Maintenance(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await factory.ResetDatabaseAsync(CancellationToken.None).ConfigureAwait(true);
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "MAINT_001 - Tạo hồ sơ phương tiện cho khách hàng")]
    public async Task CreateVehicle_ValidLead_SavesSuccessfully()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "0909123456" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var vehicle = new Vehicle 
        { 
            LeadId = lead.Id, 
            VinNumber = "VIN123", 
            EngineNumber = "ENG456", 
            LicensePlate = "59A-12345",
            PurchaseDate = DateTimeOffset.UtcNow
        };

        // Act
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        var savedVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        savedVehicle.Should().NotBeNull();
        savedVehicle!.LeadId.Should().Be(lead.Id);
    }

    [Fact(DisplayName = "MAINT_003 - Ghi nhận lịch sử bảo trì định kỳ")]
    public async Task CreateMaintenanceHistory_ValidVehicle_SavesSuccessfully()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "0909123456" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var vehicle = new Vehicle { LeadId = lead.Id, VinNumber = "V1", EngineNumber = "E1", LicensePlate = "L1" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var history = new MaintenanceHistory 
        { 
            VehicleId = vehicle.Id, 
            Mileage = 5000, 
            Description = "Thay nhớt định kỳ",
            MaintenanceDate = DateTimeOffset.UtcNow
        };

        // Act
        db.MaintenanceHistories.Add(history);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        var savedHistory = await db.MaintenanceHistories.FirstOrDefaultAsync(x => x.Id == history.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        savedHistory.Should().NotBeNull();
        savedHistory!.Mileage.Should().Be(5000);
    }

    [Fact(DisplayName = "MAINT_005 - Lưu trữ tài liệu kỹ thuật của phương tiện")]
    public async Task CreateVehicleDocument_ValidVehicle_SavesSuccessfully()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "123" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var vehicle = new Vehicle { LeadId = lead.Id, VinNumber = "V2", EngineNumber = "E2", LicensePlate = "L2" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var doc = new VehicleDocument 
        { 
            VehicleId = vehicle.Id, 
            DocumentType = "Registration", 
            FileUrl = "http://file.com/reg.pdf", 
            Description = "Giấy tờ xe" 
        };

        // Act
        db.VehicleDocuments.Add(doc);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        var savedDoc = await db.VehicleDocuments.FirstOrDefaultAsync(x => x.Id == doc.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);
        savedDoc.Should().NotBeNull();
        savedDoc!.DocumentType.Should().Be("Registration");
    }

    [Fact(DisplayName = "MAINT_006 - Truy xuất danh sách xe theo khách hàng")]
    public async Task GetVehicles_ByLeadId_ReturnsCorrectList()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "123" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        db.Vehicles.AddRange(
            new Vehicle { LeadId = lead.Id, VinNumber = "V1", EngineNumber = "E1", LicensePlate = "L1" },
            new Vehicle { LeadId = lead.Id, VinNumber = "V2", EngineNumber = "E2", LicensePlate = "L2" }
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Act
        var vehicles = await db.Vehicles.Where(v => v.LeadId == lead.Id).ToListAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        vehicles.Should().HaveCount(2);
    }

    [Fact(DisplayName = "MAINT_007 - Kiểm tra quan hệ 1-N giữa Xe và Bảo trì")]
    public async Task GetVehicle_WithMaintenanceHistories_ReturnsCorrectCount()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "123" };
        db.Leads.Add(lead);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        var vehicle = new Vehicle { LeadId = lead.Id, VinNumber = "V3", EngineNumber = "E3", LicensePlate = "L3" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        db.MaintenanceHistories.AddRange(
            new MaintenanceHistory { VehicleId = vehicle.Id, Description = "H1", MaintenanceDate = DateTimeOffset.UtcNow },
            new MaintenanceHistory { VehicleId = vehicle.Id, Description = "H2", MaintenanceDate = DateTimeOffset.UtcNow },
            new MaintenanceHistory { VehicleId = vehicle.Id, Description = "H3", MaintenanceDate = DateTimeOffset.UtcNow }
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Act
        var vehicleWithHistories = await db.Vehicles
            .Include(v => v.MaintenanceHistories)
            .FirstOrDefaultAsync(v => v.Id == vehicle.Id, TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        vehicleWithHistories.Should().NotBeNull();
        vehicleWithHistories!.MaintenanceHistories.Should().HaveCount(3);
    }

    [Fact(DisplayName = "MAINT_008 - Xóa phương tiện và các dữ liệu liên quan")]
    public async Task DeleteVehicle_SoftDeleteCascadesToRelatedData()
    {
        // Arrange
        int vehicleId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "123" };
            db.Leads.Add(lead);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            var vehicle = new Vehicle { LeadId = lead.Id, VinNumber = "V4", EngineNumber = "E4", LicensePlate = "L4" };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            vehicleId = vehicle.Id;

            db.MaintenanceHistories.Add(new MaintenanceHistory { VehicleId = vehicleId, Description = "H", MaintenanceDate = DateTimeOffset.UtcNow });
            db.VehicleDocuments.Add(new VehicleDocument { VehicleId = vehicleId, DocumentType = "D", FileUrl = "U" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        // Act
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var vehicle = await db.Vehicles
                .Include(v => v.MaintenanceHistories)
                .Include(v => v.Documents)
                .FirstOrDefaultAsync(x => x.Id == vehicleId, TestContext.Current.CancellationToken).ConfigureAwait(true);
            db.Vehicles.Remove(vehicle!);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        // Assert
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var history = await db.MaintenanceHistories.AnyAsync(h => h.VehicleId == vehicleId, TestContext.Current.CancellationToken).ConfigureAwait(true);
            var document = await db.VehicleDocuments.AnyAsync(d => d.VehicleId == vehicleId, TestContext.Current.CancellationToken).ConfigureAwait(true);

            history.Should().BeFalse();
            document.Should().BeFalse();
        }
    }

    [Fact(DisplayName = "MAINT_010 - Cập nhật thông tin biển số xe")]
    public async Task UpdateVehicle_LicensePlate_UpdatesSuccessfully()
    {
        // Arrange
        int vehicleId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            var lead = new Domain.Entities.Lead { FullName = "Owner", PhoneNumber = "123" };
            db.Leads.Add(lead);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);

            var vehicle = new Vehicle { LeadId = lead.Id, VinNumber = "V5", EngineNumber = "E5", LicensePlate = "OLD" };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
            vehicleId = vehicle.Id;
        }

        // Act
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId, TestContext.Current.CancellationToken).ConfigureAwait(true);
            vehicle!.LicensePlate = "NEW-123";
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        }

        // Assert
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            var vehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId, TestContext.Current.CancellationToken).ConfigureAwait(true);
            vehicle!.LicensePlate.Should().Be("NEW-123");
            vehicle.VinNumber.Should().Be("V5");
        }
    }

    [Fact(DisplayName = "MAINT_013 - Xác minh ràng buộc khóa ngoại LeadId")]
    public async Task CreateVehicle_InvalidLeadId_ThrowsException()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

        var vehicle = new Vehicle 
        { 
            LeadId = 9999, // ID không tồn tại
            VinNumber = "V6", 
            EngineNumber = "E6", 
            LicensePlate = "L6" 
        };

        // Act
        Func<Task> action = async () => 
        {
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync(TestContext.Current.CancellationToken).ConfigureAwait(true);
        };

        // Assert
        await action.Should().ThrowAsync<DbUpdateException>().ConfigureAwait(true);
    }
}
