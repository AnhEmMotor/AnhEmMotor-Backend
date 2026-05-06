using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeders
{
    public class VehicleTypeSeeder(ApplicationDBContext context)
    {
        public async Task SeedAsync()
        {
            if (await context.VehicleTypes.AnyAsync()) return;

            var vehicleTypes = new List<VehicleType>
            {
                new() { Name = "Xe số", Slug = "xe-so", IsActive = true, SortOrder = 1 },
                new() { Name = "Xe tay ga", Slug = "xe-tay-ga", IsActive = true, SortOrder = 2 },
                new() { Name = "Xe côn tay", Slug = "xe-con-tay", IsActive = true, SortOrder = 3 },
                new() { Name = "Xe Mô tô", Slug = "xe-mo-to", IsActive = true, SortOrder = 4 }
            };

            context.VehicleTypes.AddRange(vehicleTypes);
            await context.SaveChangesAsync();
        }
    }
}
