using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PlateDossier;
using Domain.Entities;
using MediatR;

namespace Application.Features.PlateDossiers.Commands.CreatePlateDossier
{
    public class CreatePlateDossierCommandHandler(
        IPlateDossierUpdateRepository plateDossierUpdateRepository,
        IPlateDossierReadRepository plateDossierReadRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreatePlateDossierCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreatePlateDossierCommand request, CancellationToken cancellationToken)
        {
            var existing = await plateDossierReadRepository.GetByOutputIdAsync(request.OutputId, cancellationToken)
                .ConfigureAwait(false);
            if (existing != null)
            {
                return Result<int>.Failure(Error.BadRequest("Hồ sơ biển số cho đơn hàng này đã tồn tại."));
            }
            var plateDossier = new PlateDossier
            {
                OutputId = request.OutputId,
                RegistrationFee = request.RegistrationFee,
                ActualCost = request.ActualCost,
                ServiceFee = request.ServiceFee,
                Notes = request.Notes,
                LicensePlate = request.LicensePlate ?? string.Empty,
                Status = "Prepare"
            };
            plateDossierUpdateRepository.Add(plateDossier);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(plateDossier.Id);
        }
    }
}
