using Application.ApiContracts.FinanceContract.Responses;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;

public class GetFinanceContractDetailQueryHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<GetFinanceContractDetailQuery, FinanceContractDetailResponse>
{
    public async Task<FinanceContractDetailResponse> Handle(
        GetFinanceContractDetailQuery request,
        CancellationToken cancellationToken)
    {
        // TODO: UnitOfWork hiện tại chưa expose DbContext.
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new FinanceContractDetailResponse();
    }
}

