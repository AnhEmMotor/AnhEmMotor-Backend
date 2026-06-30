using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.PlateDossiers.Queries.GetPlateDossierDetail
{
    public class GetPlateDossierDetailQuery : IRequest<Result<PlateDossierResponse>>
    {
        public int Id { get; set; }
    }
}
