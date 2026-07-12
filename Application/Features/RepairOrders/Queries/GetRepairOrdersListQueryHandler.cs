using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.HR.Employee;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Primitives;
using MediatR;

namespace Application.Features.RepairOrders.Queries;

public class GetRepairOrdersListQueryHandler(
    IMaintenanceHistoryReadRepository repo,
    IVehicleReadRepository vehicleRepo,
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
            var vehicles = await vehicleRepo.GetByIdsAsync(vehicleIds, ct);
            var vehicleDict = vehicles.ToDictionary(
                v => v.Id,
                v => !string.IsNullOrEmpty(v.LicensePlate) ? v.LicensePlate : v.VinNumber);
            var employees = await employeeRepo.GetAllWithUsersAsync(ct);
            var empDict = employees.ToDictionary(e => e.Id, e => e.User?.FullName);
            foreach (var item in paged.Items)
            {
                if (vehicleDict.TryGetValue(item.VehicleId, out var vInfo))
                    item.VehicleInfo = vInfo;
                if (item.TechnicianId.HasValue && empDict.TryGetValue(item.TechnicianId.Value, out var tName))
                    item.TechnicianName = tName;
            }
        }
        return Result<PagedResult<RepairOrderResponse>>.Success(paged);
    }
}
