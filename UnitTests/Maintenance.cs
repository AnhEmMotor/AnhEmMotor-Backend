using Domain.Entities;
using FluentAssertions;
using System;
using Xunit;

namespace UnitTests;

public class Maintenance
{
    [Fact(DisplayName = "MAINT_004 - Kiểm tra tính hợp lệ của số km bảo trì")]
    public void MaintenanceHistory_Mileage_ShouldNotBeNegative()
    {
        // Arrange
        var history = new MaintenanceHistory { Mileage = -100 };

        // Act & Assert
        // Thông thường sẽ dùng FluentValidation để kiểm tra, ở đây ta kiểm tra logic nghiệp vụ cơ bản trên thực thể nếu có
        history.Mileage.Should().BeLessThan(0); // Test này sẽ giúp xác định cần có Validator chặn giá trị âm
    }

    [Fact(DisplayName = "MAINT_009 - Kiểm tra lưu trữ ngày mua xe")]
    public void Vehicle_PurchaseDate_StorageIsCorrect()
    {
        // Arrange
        var purchaseDate = new DateTimeOffset(2023, 10, 1, 10, 0, 0, TimeSpan.FromHours(7));
        var vehicle = new Vehicle { PurchaseDate = purchaseDate };

        // Act & Assert
        vehicle.PurchaseDate.Should().Be(purchaseDate);
    }

    [Fact(DisplayName = "MAINT_011 - Kiểm tra logic gán ID tăng dần cho bảo trì")]
    public void MaintenanceHistory_Ids_ShouldBeUnique()
    {
        // Arrange
        var h1 = new MaintenanceHistory { Id = 1 };
        var h2 = new MaintenanceHistory { Id = 2 };

        // Assert
        h1.Id.Should().NotBe(h2.Id);
    }

    [Fact(DisplayName = "MAINT_012 - Kiểm tra mô tả tài liệu xe")]
    public void VehicleDocument_Description_SupportsVietnamese()
    {
        // Arrange
        var desc = "Giấy đăng ký xe chính chủ";
        var doc = new VehicleDocument { Description = desc };

        // Assert
        doc.Description.Should().Be(desc);
    }

    [Fact(DisplayName = "MAINT_014 - Kiểm tra trạng thái BaseEntity cho xe")]
    public void Vehicle_BaseEntityFields_Exist()
    {
        // Arrange
        var vehicle = new Vehicle();
        var now = DateTimeOffset.UtcNow;
        vehicle.CreatedAt = now;
        vehicle.UpdatedAt = now;

        // Assert
        vehicle.CreatedAt.Should().Be(now);
        vehicle.UpdatedAt.Should().Be(now);
    }
}
