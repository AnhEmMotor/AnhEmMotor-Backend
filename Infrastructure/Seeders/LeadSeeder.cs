using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Seeders;

public static class LeadSeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.Leads.AnyAsync()) return;

        var sales = await userManager.GetUsersInRoleAsync("Sale");
        var firstSale = sales.FirstOrDefault();

        var leads = new List<Lead>
        {
            new Lead
            {
                FullName = "Nguyễn Văn Nam",
                Email = "nam.nguyen@gmail.com",
                PhoneNumber = "0987123456",
                Status = "New",
                Source = "WebStore",
                Score = 30,
                InterestedVehicle = "Winner X 2024",
                Activities = new List<LeadActivity>
                {
                    new LeadActivity { ActivityType = "Registration", Description = "Đã đăng ký lái thử xe Winner X từ Banner ưu đãi tháng 4.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-2) },
                    new LeadActivity { ActivityType = "AI_Query", Description = "Khách hỏi AI: 'Winner X có tốn xăng không?'", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30) }
                }
            },
            new Lead
            {
                FullName = "Trần Thị Mai",
                Email = "maimai.tran@yahoo.com",
                PhoneNumber = "0912345678",
                Status = "Consulting",
                Source = "Facebook",
                Score = 65,
                InterestedVehicle = "Air Blade 160",
                AssignedToId = firstSale?.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1.5),
                Activities = new List<LeadActivity>
                {
                    new LeadActivity { ActivityType = "Inquiry", Description = "Nhắn tin hỏi về thủ tục trả góp xe Air Blade 160.", CreatedAt = DateTimeOffset.UtcNow.AddDays(-2) },
                    new LeadActivity { ActivityType = "Call", Description = "Sale đã gọi điện tư vấn nhưng khách đang bận, hẹn gọi lại sau.", CreatedAt = DateTimeOffset.UtcNow.AddDays(-1.5) }
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
                Activities = new List<LeadActivity>
                {
                    new LeadActivity { ActivityType = "Visit", Description = "Đã đến showroom lái thử xe CBR150R.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-10) },
                    new LeadActivity { ActivityType = "Process", Description = "Khách rất ưng ý, đang chờ duyệt hồ sơ vay trả góp từ ngân hàng HD Saison.", CreatedAt = DateTimeOffset.UtcNow.AddHours(-1) }
                }
            }
        };

        context.Leads.AddRange(leads);
        await context.SaveChangesAsync();
    }
}
