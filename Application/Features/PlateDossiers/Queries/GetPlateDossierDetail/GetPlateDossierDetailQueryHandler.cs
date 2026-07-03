using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.PlateDossier;
using Mapster;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.PlateDossiers.Queries.GetPlateDossierDetail
{
    public class GetPlateDossierDetailQueryHandler(IPlateDossierReadRepository repository)
        : IRequestHandler<GetPlateDossierDetailQuery, Result<PlateDossierResponse>>
    {
        public async Task<Result<PlateDossierResponse>> Handle(
            GetPlateDossierDetailQuery request,
            CancellationToken cancellationToken)
        {
            var dossier = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (dossier == null)
            {
                return Result<PlateDossierResponse>.Failure("Không tìm thấy hồ sơ biển số.");
            }

            var response = dossier.Adapt<PlateDossierResponse>();
            return Result<PlateDossierResponse>.Success(response);
        }
    }
}
