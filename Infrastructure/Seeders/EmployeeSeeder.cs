using Domain.Constants;
using Domain.Entities;
using Domain.Entities.HR;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders;

public static class EmployeeSeeder
{
    public static async Task SeedAsync(
        ApplicationDBContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var employeesToSeed = new List<(string Email, string FullName, string JobTitle, decimal Salary)>
        {
            ("nguyen.van.a@anhemmotor.com", "Nguyễn Văn A", "Trưởng phòng Kinh doanh", 25000000),
            ("tran.thi.b@anhemmotor.com", "Trần Thị B", "Chuyên viên Bán hàng", 15000000),
            ("le.van.c@anhemmotor.com", "Lê Văn C", "Kế toán tổng hợp", 18000000),
            ("pham.thi.d@anhemmotor.com", "Phạm Thị D", "Chuyên viên Tư vấn", 14000000),
            ("hoang.van.e@anhemmotor.com", "Hoàng Văn E", "Kỹ thuật viên", 12000000)
        };

        foreach (var emp in employeesToSeed)
        {
            var user = await userManager.FindByEmailAsync(emp.Email).ConfigureAwait(false);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = emp.Email,
                    Email = emp.Email,
                    FullName = emp.FullName,
                    Status = UserStatus.Active,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Employee@123456").ConfigureAwait(false);
                if (!result.Succeeded) continue;
            }

            var profile = await context.EmployeeProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken)
                .ConfigureAwait(false);

            if (profile == null)
            {
                profile = new EmployeeProfile
                {
                    UserId = user.Id,
                    JobTitle = emp.JobTitle,
                    BaseSalary = emp.Salary,
                    IdentityNumber = "031" + new Random().Next(10000000, 99999999).ToString(),
                    Address = "Biên Hòa, Đồng Nai",
                    ContractDate = DateTime.UtcNow.AddMonths(-new Random().Next(1, 24)),
                    BankName = "Vietcombank",
                    BankAccountNumber = "101" + new Random().Next(1000000, 9999999).ToString()
                };
                await context.EmployeeProfiles.AddAsync(profile, cancellationToken).ConfigureAwait(false);
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
