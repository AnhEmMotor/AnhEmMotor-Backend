using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Seeders;

public static class CommissionPolicySeeder
{
    public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
    {
        var categories = await context.ProductCategories.ToListAsync(cancellationToken);
        var motorcycleCategory = categories.FirstOrDefault(c => c.Name == "Xe máy");
        var accessoryCategory = categories.FirstOrDefault(c => c.Name == "Phụ kiện");
        var partsCategory = categories.FirstOrDefault(c => c.Name == "Phụ tùng");
        if (motorcycleCategory != null &&
            !await context.CommissionPolicies.AnyAsync(p => p.CategoryId == motorcycleCategory.Id, cancellationToken))
        {
            await context.CommissionPolicies
                .AddAsync(
                    new CommissionPolicy
                    {
                        Name = "Hoa hồng Xe máy mặc định",
                        Type = "FixedAmount",
                        Value = 500000,
                        CategoryId = motorcycleCategory.Id,
                        EffectiveDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        Unit = "xe",
                        Notes = "Mức thưởng mặc định cho tất cả các dòng xe máy."
                    },
                    cancellationToken);
        }
        if (accessoryCategory != null &&
            !await context.CommissionPolicies.AnyAsync(p => p.CategoryId == accessoryCategory.Id, cancellationToken))
        {
            await context.CommissionPolicies
                .AddAsync(
                    new CommissionPolicy
                    {
                        Name = "Hoa hồng Phụ kiện mặc định",
                        Type = "Percentage",
                        Value = 5,
                        CategoryId = accessoryCategory.Id,
                        EffectiveDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        Unit = "cái",
                        Notes = "Tính 5% trên tổng doanh thu đơn hàng phụ kiện."
                    },
                    cancellationToken);
        }
        if (partsCategory != null &&
            !await context.CommissionPolicies.AnyAsync(p => p.CategoryId == partsCategory.Id, cancellationToken))
        {
            await context.CommissionPolicies
                .AddAsync(
                    new CommissionPolicy
                    {
                        Name = "Hoa hồng Phụ tùng mặc định",
                        Type = "Percentage",
                        Value = 3,
                        CategoryId = partsCategory.Id,
                        EffectiveDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        Unit = "cái",
                        Notes = "Tính 3% trên tổng doanh thu đơn hàng phụ tùng."
                    },
                    cancellationToken);
        }
        await context.SaveChangesAsync(cancellationToken);
    }
}
