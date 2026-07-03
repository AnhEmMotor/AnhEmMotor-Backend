using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Repositories.WarrantyClaim;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WarrantyClaims.Commands.CreateWarrantyClaim
{
    public class CreateWarrantyClaimCommandHandler(
        IWarrantyClaimUpdateRepository warrantyClaimUpdateRepository,
        IVehicleReadRepository vehicleReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateWarrantyClaimCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateWarrantyClaimCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await vehicleReadRepository.GetByLicensePlateAsync(request.LicensePlate, cancellationToken)
                .ConfigureAwait(false);
            if (vehicle == null)
            {
                return Result<int>.Failure(Error.BadRequest("Không tìm thấy phương tiện với biển số này."));
            }

            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var randStr = Guid.NewGuid().ToString("N")[..4].ToUpper();
            var claimNumber = $"WAR-{todayStr}-{randStr}";

            var warrantyClaim = new WarrantyClaim
            {
                ClaimNumber = claimNumber,
                VehicleId = vehicle.Id,
                IssueDescription = request.IssueDescription,
                MediaUrls = request.MediaUrls,
                ServiceCenterName = request.ServiceCenterName,
                ManufacturerClaimNumber = request.ManufacturerClaimNumber,
                Status = WarrantyClaimStatus.Received,
                IsRecall = false,
                TotalLaborCost = 0,
                TotalPartsCost = request.Parts.Sum(p => p.UnitPrice)
            };

            foreach (var partDto in request.Parts)
            {
                warrantyClaim.Parts.Add(new WarrantyClaimPart
                {
                    PartName = partDto.PartName,
                    PartCode = partDto.PartCode,
                    UnitPrice = partDto.UnitPrice,
                    Status = WarrantyPartStatus.Pending
                });
            }

            warrantyClaimUpdateRepository.Add(warrantyClaim);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<int>.Success(warrantyClaim.Id);
        }
    }
}
