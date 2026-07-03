using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WarrantyClaim;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WarrantyClaims.Commands.UpdateWarrantyClaimStatus
{
    public class UpdateWarrantyClaimStatusCommandHandler(
        IWarrantyClaimUpdateRepository warrantyClaimUpdateRepository,
        IWarrantyClaimReadRepository warrantyClaimReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateWarrantyClaimStatusCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateWarrantyClaimStatusCommand request, CancellationToken cancellationToken)
        {
            var claim = await warrantyClaimReadRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (claim == null)
            {
                return Result<bool>.Failure("Không tìm thấy phiếu bảo hành.");
            }

            claim.Status = request.Status;
            if (request.IsRecall.HasValue)
            {
                claim.IsRecall = request.IsRecall.Value;
            }
            if (!string.IsNullOrEmpty(request.ManufacturerDecision))
            {
                claim.ManufacturerDecision = request.ManufacturerDecision;
            }

            warrantyClaimUpdateRepository.Update(claim);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
