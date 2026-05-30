using Application.Common.Models;
using Application.Interfaces.Repositories.HR.Commission;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionRecords;

public class GetCommissionRecordsQueryHandler(ICommissionReadRepository repository) : IRequestHandler<GetCommissionRecordsQuery, Result<List<CommissionRecord>>>
{
    public async Task<Result<List<CommissionRecord>>> Handle(
        GetCommissionRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var records = await repository.GetRecordsAsync(cancellationToken).ConfigureAwait(false);
        return records;
    }
}