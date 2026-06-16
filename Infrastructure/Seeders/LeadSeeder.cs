using Domain.Constants.Lead;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Infrastructure.Seeders;

public static class LeadSeeder
{
    public static async Task SeedAsync(
        ApplicationDBContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (await context.Leads.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;
        var sales = await userManager.GetUsersInRoleAsync("Sale").ConfigureAwait(false);
        var firstSale = sales.FirstOrDefault();
        var leads = new List<Lead>
        {
            new Lead
            {
                FullName = "Nguyễn Văn Nam",
                Email = "nam.nguyen@gmail.com",
                PhoneNumber = "0987123456",
                Status = LeadStatus.New,
                Source = LeadSource.WebStore,
                Score = 30,
                InterestedVehicle = "Winner X 2024",
                Activities =
                    new List<LeadActivity>
                    {
                        new LeadActivity
                        {
                            ActivityType = "Registration",
                            Description = "Đã đăng ký lái thử xe Winner X từ Banner ưu đãi tháng 4.",
                            CreatedAt = DateTimeOffset.UtcNow.AddHours(-2)
                        },
                        new LeadActivity
                        {
                            ActivityType = "AI_Query",
                            Description = "Khách hỏi AI: 'Winner X có tốn xăng không?'",
                            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30)
                        }
                    }
            },
            new Lead
            {
                FullName = "Trần Thị Mai",
                Email = "maimai.tran@yahoo.com",
                PhoneNumber = "0912345678",
                Status = LeadStatus.Consulting,
                Source = LeadSource.Facebook,
                Score = 65,
                InterestedVehicle = "Air Blade 160",
                AssignedToId = firstSale?.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1.5),
                Activities =
                    new List<LeadActivity>
                    {
                        new LeadActivity
                        {
                            ActivityType = "Inquiry",
                            Description = "Nhắn tin hỏi về thủ tục trả góp xe Air Blade 160.",
                            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
                        },
                        new LeadActivity
                        {
                            ActivityType = LeadActivityType.Call,
                            Description = "Sale đã gọi điện tư vấn nhưng khách đang bận, hẹn gọi lại sau.",
                            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1.5)
                        }
                    }
            },
            new Lead
            {
                FullName = "Lê Minh Hiếu",
                Email = "hieu.le@outlook.com",
                PhoneNumber = "0909888999",
                Status = "TestDriving",
                Source = "Shop",
                Score = 85,
                InterestedVehicle = "CBR150R Edition",
                AssignedToId = firstSale?.Id,
                Activities =
                    new List<LeadActivity>
                    {
                        new LeadActivity
                        {
                            ActivityType = "Visit",
                            Description = "Đã đến showroom lái thử xe CBR150R.",
                            CreatedAt = DateTimeOffset.UtcNow.AddHours(-10)
                        },
                        new LeadActivity
                        {
                            ActivityType = "Process",
                            Description = "Khách rất ưng ý, đang chờ duyệt hồ sơ vay trả góp từ ngân hàng HD Saison.",
                            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1)
                        }
                    }
            },
            new Lead
            {
                FullName = "Hoàng Văn Tuấn",
                Email = "tuan.hoang@gmail.com",
                PhoneNumber = "0945999888",
                Status = "Consulting",
                Source = "Shop",
                Score = 75,
                InterestedVehicle = "Honda SH 150i",
                AssignedToId = firstSale?.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-12),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new Lead
            {
                FullName = "Đỗ Thị Lan",
                Email = "lan.do@gmail.com",
                PhoneNumber = "0932888777",
                Status = "Converted",
                Source = "WebStore",
                Score = 95,
                InterestedVehicle = "Yamaha Exciter 155",
                AssignedToId = firstSale?.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-20),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-18)
            },
            new Lead
            {
                FullName = "Vũ Anh Đức",
                Email = "duc.vu@gmail.com",
                PhoneNumber = "0923777666",
                Status = "Lost",
                Source = "Facebook",
                Score = 20,
                InterestedVehicle = "Honda Winner X",
                AssignedToId = firstSale?.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-15),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-12)
            },
            new Lead
            {
                FullName = "Bùi Minh Tú",
                Email = "tu.bui@gmail.com",
                PhoneNumber = "0965123789",
                Status = "New",
                Source = "WebStore",
                Score = 45,
                InterestedVehicle = "Suzuki Raider R150",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-18)
            },
            new Lead
            {
                FullName = "Đặng Phương Thảo",
                Email = "thao.dang@gmail.com",
                PhoneNumber = "0977222333",
                Status = "Consulting",
                Source = "Facebook",
                Score = 55,
                InterestedVehicle = "Honda Vision",
                AssignedToId = firstSale?.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };
        context.Leads.AddRange(leads);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
