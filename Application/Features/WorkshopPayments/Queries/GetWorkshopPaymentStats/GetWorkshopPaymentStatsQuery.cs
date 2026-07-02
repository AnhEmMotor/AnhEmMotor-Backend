using Application.Common.Models;
using MediatR;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStats;

public class GetWorkshopPaymentStatsQuery : IRequest<Result<object>>
{
}
