using Application.Common.Models;
using Application.Interfaces.Repositories.HR.CommissionPolicy;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionPolicyById;

public class GetCommissionPolicyByIdQueryHandler(ICommissionPolicyReadRepository repository) : IRequestHandler<GetCommissionPolicyByIdQuery, Result<CommissionPolicy>>
{
    public async Task<Result<CommissionPolicy>> Handle(
        GetCommissionPolicyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var policy = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (policy == null)
            return Result<CommissionPolicy>.Failure("Chính sách không tồn tại.");
            
        return policy;
    }
}
