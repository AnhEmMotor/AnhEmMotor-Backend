using Application.Interfaces.Repositories.HR.Commission;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionRecords;

public class GetCommissionRecordsQueryHandler(ICommissionReadRepository repository) : IRequestHandler<GetCommissionRecordsQuery, List<CommissionRecord>>
{
    public Task<List<CommissionRecord>> Handle(GetCommissionRecordsQuery request, CancellationToken cancellationToken)
    {
        return repository.GetRecordsAsync(cancellationToken);
    }
}
