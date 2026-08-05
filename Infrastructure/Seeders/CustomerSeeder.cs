using Domain.Constants;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.AspNetCore.Identity;

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
            ("kimngan@gmail.com", "123456"),
            ("minhuyen@gmail.com", "123456")
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
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
