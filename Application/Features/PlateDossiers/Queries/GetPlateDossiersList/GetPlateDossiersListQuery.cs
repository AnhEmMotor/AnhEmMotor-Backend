using Application.ApiContracts.PlateDossier.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.PlateDossiers.Queries.GetPlateDossiersList
{
    public class GetPlateDossiersListQuery : IRequest<Result<PagedResult<PlateDossierResponse>>>
    {
        public SieveModel? SieveModel { get; set; }
    }
}
