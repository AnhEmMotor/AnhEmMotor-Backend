using Application.Common.Models;
using MediatR;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentDetail;

public class GetWorkshopPaymentDetailQueryHandler : IRequestHandler<GetWorkshopPaymentDetailQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetWorkshopPaymentDetailQuery request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Result<object>.Success(new { });
    }
}
