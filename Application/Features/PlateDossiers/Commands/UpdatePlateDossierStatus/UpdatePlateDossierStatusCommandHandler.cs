using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PlateDossier;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.PlateDossiers.Commands.UpdatePlateDossierStatus
{
    public class UpdatePlateDossierStatusCommandHandler(
        IPlateDossierReadRepository plateDossierReadRepository,
        IPlateDossierUpdateRepository plateDossierUpdateRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService) : IRequestHandler<UpdatePlateDossierStatusCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            UpdatePlateDossierStatusCommand request,
            CancellationToken cancellationToken)
        {
            var plateDossier = await plateDossierReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (plateDossier == null)
            {
                return Result<bool>.Failure(Error.NotFound("Hồ sơ biển số không tồn tại."));
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                plateDossier.Status = request.Status;
            }
            if (request.LicensePlate != null)
            {
                plateDossier.LicensePlate = request.LicensePlate;
            }
            if (request.RegistrationFee.HasValue)
            {
                plateDossier.RegistrationFee = request.RegistrationFee.Value;
            }
            if (request.ActualCost.HasValue)
            {
                plateDossier.ActualCost = request.ActualCost.Value;
            }
            if (request.ServiceFee.HasValue)
            {
                plateDossier.ServiceFee = request.ServiceFee.Value;
            }
            if (request.Notes != null)
            {
                plateDossier.Notes = request.Notes;
            }
            plateDossierUpdateRepository.Update(plateDossier);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // TODO(6.2/6.3): thay bằng Zalo/SMS provider thật + lưu log thông báo theo từng bước
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                notificationService.NotifyNewBooking(
                    $"PlateDossier #{plateDossier.OutputId} updated to '{plateDossier.Status}'. Customer: {plateDossier.Output?.CustomerName} ({plateDossier.Output?.CustomerPhone}).");
            }
            return Result<bool>.Success(true);
        }
    }
}
