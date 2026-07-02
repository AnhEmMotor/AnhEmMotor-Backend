using Application.Common.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentDetail;

public class GetWorkshopPaymentDetailQueryHandler : IRequestHandler<GetWorkshopPaymentDetailQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetWorkshopPaymentDetailQuery request, CancellationToken cancellationToken)
    {
        // Mock implementation
        await Task.CompletedTask;
        return Result<object>.Success(new { });
    }
}
