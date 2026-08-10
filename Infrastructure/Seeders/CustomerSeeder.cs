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
        RoleManager<ApplicationRole> roleManager,
        CancellationToken cancellationToken)
    {
        if (!await roleManager.RoleExistsAsync("Customer").ConfigureAwait(false))
        {
            await roleManager.CreateAsync(new ApplicationRole { Name = "Customer" }).ConfigureAwait(false);
        }
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

                // Tìm động sản phẩm SH để gán xe tránh lỗi hardcode ID
                var shProduct = await context.Set<Product>()
                    .Include(p => p.ProductVariants)
                    .ThenInclude(pv => pv.ProductVariantColors)
                    .FirstOrDefaultAsync(p => p.Name != null && p.Name.Contains("Honda SH"), cancellationToken);

                if (shProduct != null && shProduct.ProductVariants.Any())
                {
                    var variant = shProduct.ProductVariants.First();
                    var color = variant.ProductVariantColors.FirstOrDefault();
                    
                    if (color != null)
                    {
                        var hasVehicle = await context.Set<Vehicle>().AnyAsync(v => v.UserId == user.Id, cancellationToken);
                        if (!hasVehicle)
                        {
                            var vehicle = new Vehicle
                            {
                                UserId = user.Id,
                                LeadId = lead.Id,
                                ProductId = shProduct.Id,
                                ProductVariantId = variant.Id,
                                ProductVariantColorId = color.Id,
                                Status = "ACTIVE",
                                PurchaseDate = DateTimeOffset.UtcNow.AddMonths(-5),
                                IsActive = true,
                                LicensePlate = "59P1-123.45",
                                CurrentOdo = 5234,
                                LastMaintenanceDate = DateTime.UtcNow.AddMonths(-2),
                                NextMaintenanceDate = DateTime.UtcNow.AddMonths(1),
                                NextMaintenanceOdo = 6000,
                                ElectronicWarrantyQrCode = "AEMOTO-WARRANTY-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
                            };
                            context.Set<Vehicle>().Add(vehicle);
                            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                            // Thêm Lịch sử bảo dưỡng
                            var maintenance = new MaintenanceHistory
                            {
                                VehicleId = vehicle.Id,
                                MaintenanceNumber = "BD-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                                MaintenanceDate = DateTimeOffset.UtcNow.AddMonths(-2),
                                Description = "Bảo dưỡng định kỳ 5000km",
                                Mileage = 5000,
                                PartsCost = 150000,
                                LaborCost = 50000,
                                TotalCost = 200000,
                                ServiceType = "Bảo dưỡng",
                                NextMaintenanceDate = DateTimeOffset.UtcNow.AddMonths(1),
                                NextMaintenanceOdo = 6000
                            };
                            context.Set<MaintenanceHistory>().Add(maintenance);

                            // Thêm Hợp đồng TC
                            var finance = new FinanceContract
                            {
                                ContractNumber = "HDTC-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                                CustomerId = user.Id,
                                BankName = "FE Credit",
                                LoanAmount = 25000000,
                                TermMonths = 12,
                                InterestRate = 1.5m,
                                DisbursementStatus = "Đã giải ngân",
                                CavetLocation = "FE Credit giữ bản gốc",
                                SignedDate = DateTime.UtcNow.AddMonths(-5)
                            };
                            context.Set<FinanceContract>().Add(finance);
                            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
