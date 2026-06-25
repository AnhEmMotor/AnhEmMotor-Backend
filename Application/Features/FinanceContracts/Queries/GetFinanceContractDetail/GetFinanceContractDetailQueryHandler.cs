using Application.ApiContracts.FinanceContract.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.FinanceContract;
using Mapster;
using MediatR;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;

public sealed class GetFinanceContractDetailQueryHandler(
    IUnitOfWork unitOfWork,
    IFinanceContractReadRepository repository
) : IRequestHandler<GetFinanceContractDetailQuery, FinanceContractDetailResponse>
{
    public async Task<FinanceContractDetailResponse> Handle(
        GetFinanceContractDetailQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hợp đồng tài chính với Id = {request.Request.Id}");
        }
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Adapt<FinanceContractDetailResponse>();
    }
}
