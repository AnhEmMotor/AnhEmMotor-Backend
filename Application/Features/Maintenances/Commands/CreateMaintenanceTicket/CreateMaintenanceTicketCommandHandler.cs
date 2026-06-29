using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.MaintenanceHistory;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Maintenances.Commands.CreateMaintenanceTicket
{
    public class CreateMaintenanceTicketCommandHandler(
        IMaintenanceHistoryUpdateRepository maintenanceHistoryUpdateRepository,
        IVehicleReadRepository vehicleReadRepository,
        IVehicleUpdateRepository vehicleUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateMaintenanceTicketCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateMaintenanceTicketCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await vehicleReadRepository.GetByLicensePlateAsync(request.LicensePlate, cancellationToken)
                .ConfigureAwait(false);
            if (vehicle == null)
            {
                return Result<int>.Failure(Error.BadRequest("Không tìm thấy phương tiện với biển số này."));
            }

            var today = DateTimeOffset.UtcNow;
            var todayStr = today.ToString("yyyyMMdd");
            var randStr = Guid.NewGuid().ToString("N")[..4].ToUpper();
            var maintenanceNumber = $"MNT-{todayStr}-{randStr}";

            var partsCost = request.Parts.Sum(p => p.UnitPrice);
            var totalCost = request.LaborCost + partsCost;

            var partsJson = JsonSerializer.Serialize(request.Parts);

            var nextDate = today.AddMonths(request.CycleMonths);
            var nextOdo = request.Mileage + request.CycleKm;

            var ticket = new MaintenanceHistory
            {
                MaintenanceNumber = maintenanceNumber,
                VehicleId = vehicle.Id,
                MaintenanceDate = today,
                Mileage = request.Mileage,
                Description = request.Description,
                TechnicianId = request.TechnicianId,
                LaborCost = request.LaborCost,
                PartsCost = partsCost,
                TotalCost = totalCost,
                NextMaintenanceDate = nextDate,
                NextMaintenanceOdo = nextOdo,
                PartsJson = partsJson
            };

            // Update Vehicle info
            vehicle.CurrentOdo = request.Mileage;
            vehicle.LastMaintenanceDate = today.DateTime;
            vehicle.NextMaintenanceDate = nextDate.DateTime;
            vehicle.NextMaintenanceOdo = nextOdo;

            maintenanceHistoryUpdateRepository.Add(ticket);
            vehicleUpdateRepository.Update(vehicle);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<int>.Success(ticket.Id);
        }
    }
}
