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
    IEmployeeReadRepository employeeRepo,
    Application.Interfaces.Repositories.WorkshopPayment.IWorkshopPaymentReadRepository paymentRepo) : IRequestHandler<GetRepairOrdersListQuery, Result<PagedResult<RepairOrderResponse>>>
{
    public async Task<Result<PagedResult<RepairOrderResponse>>> Handle(
        GetRepairOrdersListQuery req,
        CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync<RepairOrderResponse>(req.Sieve, req.Mode, null, ct);
        if (paged.Items?.Any() == true)
        {
            var vehicleIds = paged.Items.Select(x => x.VehicleId).Distinct().ToList();
            var vehicles = await vehicleRepo.GetByIdsWithLeadAsync(vehicleIds, ct);
            var vehicleDict = vehicles.ToDictionary(v => v.Id, v => v);
            var employees = await employeeRepo.GetAllWithUsersAsync(ct);
            var empDict = employees.ToDictionary(e => e.Id, e => e.User?.FullName);
            
            var allPayments = await paymentRepo.GetAllAsync(ct);
            var paymentDict = allPayments.Where(x => x.SourceType == "Maintenance").ToDictionary(x => x.SourceId, x => x);
            
            foreach (var item in paged.Items)
            {
                if (paymentDict.TryGetValue(item.Id, out var pm))
                {
                    item.VoucherDiscount = pm.DiscountAmount > 0 ? pm.DiscountAmount : null;
                    item.VoucherFinalTotal = pm.TotalAmount;
                }
                else
                {
                    item.VoucherFinalTotal = item.TotalCost;
                }

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
                    else if (vehicle.User != null)
                    {
                        item.CustomerName = vehicle.User.FullName;
                        item.CustomerPhone = vehicle.User.PhoneNumber;
                    }
                }
                if (item.TechnicianId.HasValue && empDict.TryGetValue(item.TechnicianId.Value, out var tName))
                    item.TechnicianName = tName;
            }
        }
        return Result<PagedResult<RepairOrderResponse>>.Success(paged);
    }
}
