using Application.Common.Models;
using Application.Interfaces.Repositories.WorkshopPayment;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStats;

public class GetWorkshopPaymentStatsQueryHandler : IRequestHandler<GetWorkshopPaymentStatsQuery, Result<object>>
{
    private readonly IWorkshopPaymentReadRepository _repository;

    public GetWorkshopPaymentStatsQueryHandler(IWorkshopPaymentReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<object>> Handle(GetWorkshopPaymentStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _repository.GetStatsAsync(cancellationToken);
        return Result<object>.Success(stats);
    }
}
