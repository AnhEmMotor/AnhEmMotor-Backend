using Application.ApiContracts.WarrantyTerms.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyTerm;
using Mapster;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermById;

public class GetWarrantyTermByIdQueryHandler(IWarrantyTermReadRepository readRepository) : IRequestHandler<GetWarrantyTermByIdQuery, Result<WarrantyTermResponse>>
{
    public async Task<Result<WarrantyTermResponse>> Handle(
        GetWarrantyTermByIdQuery request,
        CancellationToken cancellationToken)
    {
        var term = await readRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (term == null)
            return Error.NotFound("Warranty term not found.");
        return Result<WarrantyTermResponse>.Success(term.Adapt<WarrantyTermResponse>());
    }
}
