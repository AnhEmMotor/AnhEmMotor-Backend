                                                                                                                                          using Application.ApiContracts.FinanceContract.Responses;
using Application.Common.Helper;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;

public sealed class GetFinanceContractDetailQueryHandler(
    IUnitOfWork unitOfWork
) : IRequestHandler<GetFinanceContractDetailQuery, FinanceContractDetailResponse>
{
    public async Task<FinanceContractDetailResponse> Handle(
        GetFinanceContractDetailQuery request,
        CancellationToken cancellationToken)
    {
        // TODO: UnitOfWork hiện tại chưa expose DbContext.
        // Tạm thời lấy trực tiếp từ EF core sẽ được bổ sung sau.
        // Hiện làm placeholder để endpoint compile.
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new FinanceContractDetailResponse();



    }
}

