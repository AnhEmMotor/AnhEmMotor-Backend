using Domain.Entities;
using Domain.Enums;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;

namespace Infrastructure.Seeders;

public static class DashboardDataSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var existingLeads = await context.Leads.CountAsync(cancellationToken);
        if (existingLeads < 10)
        {
            var leads = new List<Lead>
            {
                new()
                {
                    FullName = "Đỗ Minh Quân",
                    Email = "an.nguyen@gmail.com",
                    PhoneNumber = "0901234567",
                    Score = 95,
                    Status = "Consulting",
                    Source = "Facebook",
                    InterestedVehicle = "Honda Winner X",
                    Notes = "Khách quan tâm xe đua",
                    Priority = "High",
                    Address = "Biên Hòa",
                    AddressDetail = "123 Đường A",
                    Ward = "Hòa Bình",
                    District = "Biên Hòa",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "Silver",
                    Points = 150,
                    IsVerified = true,
                    CreatedAt = now.AddDays(-5)
                },
                new()
                {
                    FullName = "Trần Thị Maiình",
                    Email = "binh.tran@gmail.com",
                    PhoneNumber = "0912345678",
                    Score = 92,
                    Status = "Consulting",
                    Source = "WebStore",
                    InterestedVehicle = "Honda SH 160",
                    Notes = "Muốn mua trả góp",
                    Priority = "High",
                    Address = "Biên Hòa",
                    AddressDetail = "456 Đường B",
                    Ward = "Tân Phong",
                    District = "Biên Hòa",
                    Province = "Đồng Nai",
                    Gender = "Female",
                    Tier = "Gold",
                    Points = 200,
                    IsVerified = true,
                    CreatedAt = now.AddDays(-3)
                },
                new()
                {
                    FullName = "Lê Minh Cường",
                    Email = "cuong.le@gmail.com",
                    PhoneNumber = "0923456789",
                    Score = 88,
                    Status = "TestDriving",
                    Source = "Shop",
                    InterestedVehicle = "Yamaha Exciter 155",
                    Notes = "Đã lái thử, hài lòng",
                    Priority = "High",
                    Address = "Biên Hòa",
                    AddressDetail = "789 Đường C",
                    Ward = "Tân Mai",
                    District = "Biên Hòa",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "Silver",
                    Points = 120,
                    IsVerified = true,
                    CreatedAt = now.AddDays(-1)
                },
                new()
                {
                    FullName = "Phạm Thị Dung",
                    Email = "dung.pham@gmail.com",
                    PhoneNumber = "0934567890",
                    Score = 85,
                    Status = "Consulting",
                    Source = "Facebook",
                    InterestedVehicle = "Honda Vision",
                    Notes = "Cần tư vấn thêm màu sắc",
                    Priority = "High",
                    Address = "Long Khánh",
                    AddressDetail = "12 Trần Phú",
                    Ward = "Xuân An",
                    District = "Long Khánh",
                    Province = "Đồng Nai",
                    Gender = "Female",
                    Tier = "Silver",
                    Points = 90,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-2)
                },
                new()
                {
                    FullName = "Hoàng Văn Em",
                    Email = "em.hoang@gmail.com",
                    PhoneNumber = "0945678901",
                    Score = 82,
                    Status = "Consulting",
                    Source = "WebStore",
                    InterestedVehicle = "Honda Air Blade",
                    Notes = "Hỏi về chương trình KM",
                    Priority = "High",
                    Address = "Nhơn Trạch",
                    AddressDetail = "34 Lê Lợi",
                    Ward = "Phước Thiền",
                    District = "Nhơn Trạch",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "NewMember",
                    Points = 50,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-4)
                },
                new()
                {
                    FullName = "Vũ Thị Phương",
                    Email = "phuong.vu@gmail.com",
                    PhoneNumber = "0956789012",
                    Score = 70,
                    Status = "New",
                    Source = "WebStore",
                    InterestedVehicle = "Honda PCX 160",
                    Notes = "Mới đăng ký tìm hiểu",
                    Priority = "Medium",
                    Address = "Trảng Bom",
                    AddressDetail = "56 Nguyễn Huệ",
                    Ward = "Hố Nai 3",
                    District = "Trảng Bom",
                    Province = "Đồng Nai",
                    Gender = "Female",
                    Tier = "NewMember",
                    Points = 20,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-7)
                },
                new()
                {
                    FullName = "Đặng Quốc Giang",
                    Email = "giang.dang@gmail.com",
                    PhoneNumber = "0967890123",
                    Score = 65,
                    Status = "New",
                    Source = "Facebook",
                    InterestedVehicle = "Yamaha NVX",
                    Notes = "Quan tâm xe tay ga",
                    Priority = "Medium",
                    Address = "Biên Hòa",
                    AddressDetail = "78 Bùi Thị Xuân",
                    Ward = "Tân Biên",
                    District = "Biên Hòa",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "NewMember",
                    Points = 10,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-10)
                },
                new()
                {
                    FullName = "Bùi Thị Hoa",
                    Email = "hoa.bui@gmail.com",
                    PhoneNumber = "0978901234",
                    Score = 55,
                    Status = "Consulting",
                    Source = "Shop",
                    InterestedVehicle = "Honda CB150R",
                    Notes = "Thích xe côn tay",
                    Priority = "Medium",
                    Address = "Long Thành",
                    AddressDetail = "90 Trần Hưng Đạo",
                    Ward = "Long Thành",
                    District = "Long Thành",
                    Province = "Đồng Nai",
                    Gender = "Female",
                    Tier = "NewMember",
                    Points = 30,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-15)
                },
                new()
                {
                    FullName = "Ngô Văn Ích",
                    Email = "ich.ngo@gmail.com",
                    PhoneNumber = "0989012345",
                    Score = 40,
                    Status = "New",
                    Source = "WebStore",
                    InterestedVehicle = "Honda Future 125",
                    Notes = "Chỉ xem thông tin",
                    Priority = "Low",
                    Address = "Cẩm Mỹ",
                    AddressDetail = "11 Quốc lộ 1A",
                    Ward = "Sông Nhạn",
                    District = "Cẩm Mỹ",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "NewMember",
                    Points = 0,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-20)
                },
                new()
                {
                    FullName = "Trịnh Thị Kim",
                    Email = "kim.trinh@gmail.com",
                    PhoneNumber = "0990123456",
                    Score = 30,
                    Status = "Lost",
                    Source = "Facebook",
                    InterestedVehicle = "Honda Wave Alpha",
                    Notes = "Không còn liên lạc",
                    Priority = "Low",
                    Address = "Định Quán",
                    AddressDetail = "22 Đinh Tiên Hoàng",
                    Ward = "Gia Canh",
                    District = "Định Quán",
                    Province = "Đồng Nai",
                    Gender = "Female",
                    Tier = "NewMember",
                    Points = 0,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-30)
                },
                new()
                {
                    FullName = "Lý Văn Long",
                    Email = "long.ly@gmail.com",
                    PhoneNumber = "0901234568",
                    Score = 25,
                    Status = "Lost",
                    Source = "Shop",
                    InterestedVehicle = "Honda XR150L",
                    Notes = "Đã mua ở nơi khác",
                    Priority = "Low",
                    Address = "Tân Phú",
                    AddressDetail = "33 Lê Duẩn",
                    Ward = "Tân Phú",
                    District = "Tân Phú",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "NewMember",
                    Points = 0,
                    IsVerified = false,
                    CreatedAt = now.AddDays(-45)
                },
                new()
                {
                    FullName = "Mai Thị Mỹ",
                    Email = "my.mai@gmail.com",
                    PhoneNumber = "0912345679",
                    Score = 80,
                    Status = "Consulting",
                    Source = "Facebook",
                    InterestedVehicle = "Honda Lead 125",
                    Notes = "Đang xem xét mua tặng vợ",
                    Priority = "High",
                    Address = "Biên Hòa",
                    AddressDetail = "44 Phạm Văn Thuận",
                    Ward = "Bình Đa",
                    District = "Biên Hòa",
                    Province = "Đồng Nai",
                    Gender = "Male",
                    Tier = "Silver",
                    Points = 80,
                    IsVerified = true,
                    CreatedAt = now.AddDays(-6)
                },
            };
            context.Leads.AddRange(leads);
            await context.SaveChangesAsync(cancellationToken);
        }
        var hasExpenseTable = false;
        try
        {
            hasExpenseTable = await context.Expenses.AnyAsync(cancellationToken);
        } catch
        {
            hasExpenseTable = false;
        }
        if (hasExpenseTable)
        {
            var existingExpenses = await context.Expenses.CountAsync(cancellationToken);
            if (existingExpenses < 5)
            {
                var today = DateTime.UtcNow.Date;
                var nowDt = DateTime.UtcNow;
                context.Expenses
                    .AddRange(
                        new Expense
                        {
                            Name = "Tiền thuê mặt bằng tháng 6",
                            Amount = 25_000_000,
                            ExpenseDate = today,
                            Category = ExpenseCategory.Fixed,
                            Note = "Thuê showroom tháng 6/2026",
                            CreatedAt = nowDt
                        },
                        new Expense
                        {
                            Name = "Lương nhân viên tháng 6",
                            Amount = 180_000_000,
                            ExpenseDate = today,
                            Category = ExpenseCategory.Variable,
                            Note = "Bao gồm thưởng và phụ cấp",
                            CreatedAt = nowDt
                        },
                        new Expense
                        {
                            Name = "Chi phí điện nước",
                            Amount = 5_500_000,
                            ExpenseDate = today.AddDays(-1),
                            Category = ExpenseCategory.Variable,
                            Note = "Điện + nước tháng 6",
                            CreatedAt = nowDt.AddDays(-1)
                        },
                        new Expense
                        {
                            Name = "Chi phí marketing Facebook",
                            Amount = 12_000_000,
                            ExpenseDate = today.AddDays(-2),
                            Category = ExpenseCategory.Variable,
                            Note = "Chạy ads tháng 6/2026",
                            CreatedAt = nowDt.AddDays(-2)
                        },
                        new Expense
                        {
                            Name = "Bảo dưỡng thiết bị xưởng",
                            Amount = 8_000_000,
                            ExpenseDate = today.AddDays(-5),
                            Category = ExpenseCategory.Variable,
                            Note = "Bảo dưỡng định kỳ",
                            CreatedAt = nowDt.AddDays(-5)
                        });
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        var existingActivities = await context.LeadActivities.CountAsync(cancellationToken);
        if (existingActivities < 10)
        {
            var activityNow = DateTime.UtcNow;
            var quan = await context.Leads.FirstOrDefaultAsync(l => l.FullName == "Đỗ Minh Quân", cancellationToken);
            if (quan != null)
            {
                context.LeadActivities
                    .AddRange(
                        new LeadActivity
                        {
                            LeadId = quan.Id,
                            ActivityType = "Call",
                            Description = "Gọi điện tư vấn gói trả góp Winner X 0%",
                            CreatedAt = activityNow.AddHours(-1)
                        },
                        new LeadActivity
                        {
                            LeadId = quan.Id,
                            ActivityType = "Visit",
                            Description = "Khách đến xem xe trực tiếp tại showroom",
                            CreatedAt = activityNow.AddHours(-3)
                        },
                        new LeadActivity
                        {
                            LeadId = quan.Id,
                            ActivityType = "Note",
                            Description = "Khách hài lòng với màu đỏ đen",
                            CreatedAt = activityNow.AddHours(-5)
                        });
            }
            var mai = await context.Leads.FirstOrDefaultAsync(l => l.FullName == "Trần Thị Maiình", cancellationToken);
            if (mai != null)
            {
                context.LeadActivities
                    .AddRange(
                        new LeadActivity
                        {
                            LeadId = mai.Id,
                            ActivityType = "Email",
                            Description = "Gửi báo giá Honda SH 160 kèm ưu đãi",
                            CreatedAt = activityNow.AddHours(-2)
                        },
                        new LeadActivity
                        {
                            LeadId = mai.Id,
                            ActivityType = "Call",
                            Description = "Tư vấn về thủ tục trả góp 36 tháng",
                            CreatedAt = activityNow.AddHours(-6)
                        });
            }
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
