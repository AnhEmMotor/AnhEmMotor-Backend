using Application.Interfaces.Repositories.HR;
using Domain.Entities.HR;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.HR.Queries.GetCommissionRecords;

public class GetCommissionRecordsQueryHandler(ICommissionReadRepository repository)
    : IRequestHandler<GetCommissionRecordsQuery, List<CommissionRecord>>
{
    public Task<List<CommissionRecord>> Handle(
        GetCommissionRecordsQuery request,
        CancellationToken cancellationToken)
    {
        return repository.GetRecordsAsync(cancellationToken);
    }
}
