using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders;

public static class CustomerSeeder
{
    public static async Task SeedAsync(
        ApplicationDBContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var customersToSeed = new List<(string Email, string Password)>
        {
            ("kimngan@gmail.com", "Customer@123456"),
            ("minhuyen@gmail.com", "Customer@123456")
        };
        foreach (var (Email, Password) in customersToSeed)
        {
            var user = await userManager.FindByEmailAsync(Email).ConfigureAwait(false);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = Email,
                    Email = Email,
                    FullName = Email.Split('@')[0],
                    Status = UserStatus.Active,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, Password).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer").ConfigureAwait(false);
                }
            }
            
            if (Email == "minhuyen@gmail.com")
            {
                var lead = await context.Set<Lead>().FirstOrDefaultAsync(l => l.Email == Email, cancellationToken);
                if (lead == null)
                {
                    lead = new Lead
                    {
                        FullName = "Minh Huyền",
                        Email = Email,
                        PhoneNumber = "0987654321"
                    };
                    context.Set<Lead>().Add(lead);
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var userId = user.Id;
                var vehicle = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);
                if (vehicle == null)
                {
                    vehicle = new Vehicle
                    {
                        LeadId = lead.Id,
                        UserId = userId,
                        ProductId = 68,
                        VinNumber = "VIN-MINHUYEN-001",
                        EngineNumber = "ENG-MINHUYEN-001",
                        LicensePlate = "59A1-999.99",
                        CurrentOdo = 1200,
                        PurchaseDate = DateTimeOffset.UtcNow
                    };
                    context.Set<Vehicle>().Add(vehicle);
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
