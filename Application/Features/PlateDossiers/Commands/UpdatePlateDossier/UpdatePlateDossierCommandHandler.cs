using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PlateDossier;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PlateDossiers.Commands.UpdatePlateDossier
{
    public class UpdatePlateDossierCommandHandler(
        IPlateDossierReadRepository plateDossierReadRepository,
        IPlateDossierUpdateRepository plateDossierUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdatePlateDossierCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdatePlateDossierCommand request, CancellationToken cancellationToken)
        {
            var dossier = await plateDossierReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (dossier == null)
            {
                return Result<bool>.Failure("Không tìm thấy hồ sơ biển số.");
            }

            dossier.CustomerName = request.CustomerName;
            dossier.CustomerPhone = request.CustomerPhone;
            dossier.LicensePlate = request.LicensePlate;
            dossier.VinNumber = request.VinNumber;
            dossier.Status = request.Status;
            dossier.RegistrationFee = request.RegistrationFee;
            dossier.ActualCost = request.ActualCost;
            dossier.ServiceFee = request.ServiceFee;
            dossier.Notes = request.Notes;

            if (request.Status == "Hoàn thành")
            {
                dossier.CompletedDate = request.CompletedDate ?? DateTimeOffset.UtcNow;
            }
            else
            {
                dossier.CompletedDate = null;
            }

            plateDossierUpdateRepository.Update(dossier);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
