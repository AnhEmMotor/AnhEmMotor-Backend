using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.PlateDossier;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PlateDossiers.Commands.DeletePlateDossier
{
    public class DeletePlateDossierCommandHandler(
        IPlateDossierReadRepository plateDossierReadRepository,
        IPlateDossierUpdateRepository plateDossierUpdateRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeletePlateDossierCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeletePlateDossierCommand request, CancellationToken cancellationToken)
        {
            var dossier = await plateDossierReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (dossier == null)
            {
                return Result<bool>.Failure("Không tìm thấy hồ sơ biển số.");
            }

            plateDossierUpdateRepository.Remove(dossier);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
    }
}
