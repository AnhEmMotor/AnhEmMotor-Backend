using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Domain.Primitives;
using Infrastructure.DBContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrdersListQueryHandler(
    IMaintenanceHistoryReadRepository repo,
    ApplicationDBContext dbContext,
    IEmployeeReadRepository employeeRepo) : IRequestHandler<GetRepairOrdersListQuery, Result<PagedResult<RepairOrderResponse>>>
{
    public async Task<Result<PagedResult<RepairOrderResponse>>> Handle(
        GetRepairOrdersListQuery req,
        CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync<RepairOrderResponse>(req.Sieve, req.Mode, null, ct);
        if (paged.Items?.Any() == true)
        {
            var vehicleIds = paged.Items.Select(x => x.VehicleId).Distinct().ToList();
            var vehicles = await dbContext.Vehicles
                .IgnoreQueryFilters()
                .Include(v => v.Lead)
                .Where(v => vehicleIds.Contains(v.Id))
                .ToListAsync(ct);
            var vehicleDict = vehicles.ToDictionary(v => v.Id, v => v);
            var employees = await employeeRepo.GetAllWithUsersAsync(ct);
            var empDict = employees.ToDictionary(e => e.Id, e => e.User?.FullName);
            foreach (var item in paged.Items)
            {
                if (vehicleDict.TryGetValue(item.VehicleId, out var vehicle))
                {
                    item.VehicleInfo = !string.IsNullOrEmpty(vehicle.LicensePlate)
                        ? vehicle.LicensePlate
                        : vehicle.VinNumber;
                    if (vehicle.Lead != null)
                    {
                        item.CustomerName = vehicle.Lead.FullName;
                        item.CustomerPhone = vehicle.Lead.PhoneNumber;
                    }
                }
                if (item.TechnicianId.HasValue && empDict.TryGetValue(item.TechnicianId.Value, out var tName))
                    item.TechnicianName = tName;
            }
        }
        return Result<PagedResult<RepairOrderResponse>>.Success(paged);
    }
}
