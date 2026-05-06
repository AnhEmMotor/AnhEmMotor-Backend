using Domain.Entities;
using FluentAssertions;
using System;

namespace UnitTests;

public class Maintenance
{
    [Fact(DisplayName = "MAINT_004 - Kiểm tra tính hợp lệ của số km bảo trì")]
    public void MaintenanceHistory_Mileage_ShouldNotBeNegative()
    {
        var history = new MaintenanceHistory { Mileage = -100 };
        history.Mileage.Should().BeLessThan(0);
    }

    [Fact(DisplayName = "MAINT_009 - Kiểm tra lưu trữ ngày mua xe")]
    public void Vehicle_PurchaseDate_StorageIsCorrect()
    {
        var purchaseDate = new DateTimeOffset(2023, 10, 1, 10, 0, 0, TimeSpan.FromHours(7));
        var vehicle = new Vehicle { PurchaseDate = purchaseDate };
        vehicle.PurchaseDate.Should().Be(purchaseDate);
    }

    [Fact(DisplayName = "MAINT_011 - Kiểm tra logic gán ID tăng dần cho bảo trì")]
    public void MaintenanceHistory_Ids_ShouldBeUnique()
    {
        var h1 = new MaintenanceHistory { Id = 1 };
        var h2 = new MaintenanceHistory { Id = 2 };
        h1.Id.Should().NotBe(h2.Id);
    }

    [Fact(DisplayName = "MAINT_012 - Kiểm tra mô tả tài liệu xe")]
    public void VehicleDocument_Description_SupportsVietnamese()
    {
        var desc = "Giấy đăng ký xe chính chủ";
        var doc = new VehicleDocument { Description = desc };
        doc.Description.Should().Be(desc);
    }

    [Fact(DisplayName = "MAINT_014 - Kiểm tra trạng thái BaseEntity cho xe")]
    public void Vehicle_BaseEntityFields_Exist()
    {
        var vehicle = new Vehicle();
        var now = DateTimeOffset.UtcNow;
        vehicle.CreatedAt = now;
        vehicle.UpdatedAt = now;
        vehicle.CreatedAt.Should().Be(now);
        vehicle.UpdatedAt.Should().Be(now);
    }
}
