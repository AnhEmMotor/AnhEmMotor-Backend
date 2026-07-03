using Domain.Constants.Order;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders
{
    public static class WorkshopAndServiceSeeder
    {
        public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra xem đã có dữ liệu trong các bảng Workshop & Service chưa
            if (await context.WarrantyClaims.AnyAsync(cancellationToken).ConfigureAwait(false) ||
                await context.MaintenanceHistories.AnyAsync(cancellationToken).ConfigureAwait(false) ||
                await context.PlateDossiers.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            // 2. Đảm bảo có dữ liệu xe (Vehicle) để làm việc
            var vehicles = await context.Vehicles.ToListAsync(cancellationToken).ConfigureAwait(false);
            if (vehicles.Count == 0)
            {
                // Lấy các Leads có sẵn từ LeadSeeder làm khách hàng đại diện
                var leads = await context.Leads.ToListAsync(cancellationToken).ConfigureAwait(false);
                var variants = await context.ProductVariants
                    .Include(v => v.ProductVariantColors)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (leads.Count > 0 && variants.Count > 0)
                {
                    var lead1 = leads.FirstOrDefault(l => l.PhoneNumber == "0987123456") ?? leads[0];
                    var lead2 = leads.FirstOrDefault(l => l.PhoneNumber == "0912345678") ?? (leads.Count > 1 ? leads[1] : leads[0]);
                    var lead3 = leads.FirstOrDefault(l => l.PhoneNumber == "0909888999") ?? (leads.Count > 2 ? leads[2] : leads[0]);

                    var variant1 = variants[0];
                    var variant2 = variants.Count > 1 ? variants[1] : variants[0];
                    var variant3 = variants.Count > 2 ? variants[2] : variants[0];

                    var v1 = new Vehicle
                    {
                        LeadId = lead1.Id,
                        ProductId = variant1.ProductId,
                        ProductVariantId = variant1.Id,
                        ProductVariantColorId = variant1.ProductVariantColors.FirstOrDefault()?.Id,
                        VinNumber = "VINWINNERX2024SAMPLE",
                        EngineNumber = "ENGWINNERX2024SAMPLE",
                        LicensePlate = "29-H1 123.45",
                        CurrentOdo = 15000,
                        Status = VehicleStatus.Sold,
                        PurchaseDate = now.AddYears(-1),
                        IsActive = true
                    };

                    var v2 = new Vehicle
                    {
                        LeadId = lead2.Id,
                        ProductId = variant2.ProductId,
                        ProductVariantId = variant2.Id,
                        ProductVariantColorId = variant2.ProductVariantColors.FirstOrDefault()?.Id,
                        VinNumber = "VINAIRBLADE160SAMPLE",
                        EngineNumber = "ENGAIRBLADE160SAMPLE",
                        LicensePlate = "29-C1 567.89",
                        CurrentOdo = 32000,
                        Status = VehicleStatus.Sold,
                        PurchaseDate = now.AddYears(-4),
                        IsActive = true
                    };

                    var v3 = new Vehicle
                    {
                        LeadId = lead3.Id,
                        ProductId = variant3.ProductId,
                        ProductVariantId = variant3.Id,
                        ProductVariantColorId = variant3.ProductVariantColors.FirstOrDefault()?.Id,
                        VinNumber = "VINCBR150RSAMPLE",
                        EngineNumber = "ENGCBR150RSAMPLE",
                        LicensePlate = "29-F1 999.99",
                        CurrentOdo = 800,
                        Status = VehicleStatus.Sold,
                        PurchaseDate = now.AddMonths(-1),
                        IsActive = true
                    };

                    context.Vehicles.AddRange(new[] { v1, v2, v3 });
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    vehicles = new List<Vehicle> { v1, v2, v3 };
                }
            }

            if (vehicles.Count == 0) return;

            var vehicle1 = vehicles[0];
            var vehicle2 = vehicles.Count > 1 ? vehicles[1] : vehicles[0];
            var vehicle3 = vehicles.Count > 2 ? vehicles[2] : vehicles[0];

            // Lấy danh sách kỹ thuật viên để phân công
            var technicians = await context.EmployeeProfiles.ToListAsync(cancellationToken).ConfigureAwait(false);
            var techId = technicians.FirstOrDefault()?.Id;

            // 3. Seed dữ liệu Đăng ký Biển số (PlateDossier)
            var dossiers = new List<PlateDossier>
            {
                new()
                {
                    DossierNumber = "PD-" + now.AddDays(-15).ToString("yyyyMMdd") + "-0001",
                    CustomerName = "Nguyễn Văn Nam",
                    CustomerPhone = "0987123456",
                    VinNumber = vehicle1.VinNumber,
                    LicensePlate = vehicle1.LicensePlate,
                    RegistrationFee = 1500000,
                    ServiceFee = 500000,
                    ActualCost = 1800000,
                    Status = "Completed",
                    CompletedDate = now.AddDays(-10),
                    Notes = "Hồ sơ hoàn tất nhanh, khách hàng nhận cà-vẹt đúng hẹn.",
                    CreatedAt = now.AddDays(-15)
                },
                new()
                {
                    DossierNumber = "PD-" + now.AddDays(-3).ToString("yyyyMMdd") + "-0002",
                    CustomerName = "Trần Thị Mai",
                    CustomerPhone = "0912345678",
                    VinNumber = vehicle2.VinNumber,
                    LicensePlate = vehicle2.LicensePlate,
                    RegistrationFee = 2000000,
                    ServiceFee = 600000,
                    ActualCost = 2400000,
                    Status = "PlateAssigned",
                    Notes = "Đã nộp thuế và bấm số thành công, đang chờ giấy hẹn trả cà-vẹt.",
                    CreatedAt = now.AddDays(-3)
                },
                new()
                {
                    DossierNumber = "PD-" + now.AddHours(-5).ToString("yyyyMMdd") + "-0003",
                    CustomerName = "Lê Minh Hiếu",
                    CustomerPhone = "0909888999",
                    VinNumber = vehicle3.VinNumber,
                    LicensePlate = vehicle3.LicensePlate,
                    RegistrationFee = 3500000,
                    ServiceFee = 800000,
                    ActualCost = 0,
                    Status = "Prepare",
                    Notes = "Khách mới gửi hồ sơ, bộ phận dịch vụ đang chuẩn bị tờ khai thuế trước bạ.",
                    CreatedAt = now.AddHours(-5)
                }
            };
            context.PlateDossiers.AddRange(dossiers);

            // 4. Seed dữ liệu Phiếu Bảo hành (WarrantyClaim)
            var warrantyClaims = new List<WarrantyClaim>
            {
                new()
                {
                    ClaimNumber = "WAR-" + now.AddDays(-10).ToString("yyyyMMdd") + "-01",
                    VehicleId = vehicle3.Id, // CBR150R mới, còn bảo hành
                    IssueDescription = "Cụm giảm xóc trước có hiện tượng rò rỉ dầu nhớt làm ướt ty phuộc.",
                    ServiceCenterName = "Chi nhánh Đống Đa - AnhEmMotor",
                    Status = WarrantyClaimStatus.Received,
                    TotalLaborCost = 0,
                    TotalPartsCost = 0,
                    CreatedAt = now.AddDays(-10),
                    Parts = new List<WarrantyClaimPart>
                    {
                        new()
                        {
                            PartName = "Phớt dầu giảm xóc trước CBR150R",
                            PartCode = "SHOCK-SEAL-CBR",
                            UnitPrice = 0,
                            Status = WarrantyPartStatus.Pending
                        }
                    }
                },
                new()
                {
                    ClaimNumber = "WAR-" + now.AddDays(-2).ToString("yyyyMMdd") + "-02",
                    VehicleId = vehicle1.Id, // Winner X còn bảo hành
                    IssueDescription = "Đèn xi nhan trước bên phải nhấp nháy chập chờn khi rẽ.",
                    ServiceCenterName = "Trung tâm Bảo hành AnhEmMotor",
                    Status = WarrantyClaimStatus.Approved,
                    TotalLaborCost = 0,
                    TotalPartsCost = 120000,
                    CreatedAt = now.AddDays(-2),
                    Parts = new List<WarrantyClaimPart>
                    {
                        new()
                        {
                            PartName = "Cụm đèn xi nhan trước Winner X",
                            PartCode = "TURNSIGNAL-WINNERX",
                            UnitPrice = 0,
                            Status = WarrantyPartStatus.Approved
                        },
                        new()
                        {
                            PartName = "Rơ le nháy xi nhan phát sinh ngoài diện BH",
                            PartCode = "FLASHER-RELAY-12V",
                            UnitPrice = 120000,
                            Status = WarrantyPartStatus.Approved
                        }
                    }
                }
            };
            context.WarrantyClaims.AddRange(warrantyClaims);

            // 5. Seed dữ liệu Phiếu Bảo trì (MaintenanceHistory)
            var parts1 = new[]
            {
                new { PartName = "Dầu nhớt Motul 7100 10W40", PartCode = "OIL-MOTUL-7100", UnitPrice = 280000 },
                new { PartName = "Lọc dầu nhớt Winner X chính hãng", PartCode = "FILTER-OIL-WINNER", UnitPrice = 90000 }
            };
            var parts2 = new[]
            {
                new { PartName = "Dầu láp xe ga Honda", PartCode = "OIL-GEAR-HONDA", UnitPrice = 60000 },
                new { PartName = "Lọc gió AirBlade 160", PartCode = "FILTER-AIR-AB160", UnitPrice = 110000 },
                new { PartName = "Bugi NGK Iridium", PartCode = "SPARK-NGK-IRIDIUM", UnitPrice = 220000 }
            };

            var maintenanceRecords = new List<MaintenanceHistory>
            {
                new()
                {
                    MaintenanceNumber = "MNT-" + now.AddMonths(-3).ToString("yyyyMMdd") + "-01",
                    VehicleId = vehicle1.Id,
                    MaintenanceDate = now.AddMonths(-3),
                    Mileage = 12000,
                    Description = "Bảo dưỡng định kỳ mốc 12.000 km. Thay nhớt động cơ, lọc nhớt, vệ sinh buồng đốt.",
                    TechnicianId = techId,
                    LaborCost = 150000,
                    PartsCost = 370000,
                    TotalCost = 520000,
                    NextMaintenanceDate = now.AddMonths(3),
                    NextMaintenanceOdo = 17000,
                    PartsJson = JsonSerializer.Serialize(parts1),
                    CreatedAt = now.AddMonths(-3)
                },
                new()
                {
                    MaintenanceNumber = "MNT-" + now.AddDays(-15).ToString("yyyyMMdd") + "-02",
                    VehicleId = vehicle2.Id,
                    MaintenanceDate = now.AddDays(-15),
                    Mileage = 30000,
                    Description = "Bảo dưỡng mốc lớn 30.000 km. Kiểm tra hệ thống côn truyền động, thay dầu láp xe ga, bugi và lọc gió.",
                    TechnicianId = techId,
                    LaborCost = 250000,
                    PartsCost = 390000,
                    TotalCost = 640000,
                    NextMaintenanceDate = now.AddMonths(5),
                    NextMaintenanceOdo = 35000,
                    PartsJson = JsonSerializer.Serialize(parts2),
                    CreatedAt = now.AddDays(-15)
                }
            };
            context.MaintenanceHistories.AddRange(maintenanceRecords);

            // Lưu tất cả dữ liệu mẫu xuống cơ sở dữ liệu
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
