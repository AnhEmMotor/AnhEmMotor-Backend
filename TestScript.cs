using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Repositories.Vehicle;
using Application.ApiContracts.Vehicle.Responses;
using Sieve.Models;
using WebAPI;

public class TestRepository {
    public static async Task Run(IServiceProvider provider) {
        var repo = provider.GetRequiredService<IVehicleReadRepository>();
        var db = provider.GetRequiredService<Infrastructure.DBContexts.ApplicationDBContext>();
        
        var leadQuery = db.Leads.Where(l => l.PhoneNumber.Contains("0987123456")).ToList();
        Console.WriteLine($"Found {leadQuery.Count} Leads containing 0987123456");

        var query = db.Vehicles.Where(v => v.Lead != null && v.Lead.PhoneNumber.Contains("0987123456")).ToList();
        Console.WriteLine($"Found {query.Count} Vehicles for that phone number manually");

        try {
            var result = await repo.GetPagedAsync<VehicleResponse>(
                new SieveModel { Filters = "" },
                Domain.Constants.DataFetchMode.ActiveOnly,
                v => v.Lead!.PhoneNumber.Contains("0987123456"),
                default);
            Console.WriteLine($"GetPagedAsync success: {result?.Items?.Count} items.");
        } catch (Exception ex) {
            Console.WriteLine($"Exception: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
