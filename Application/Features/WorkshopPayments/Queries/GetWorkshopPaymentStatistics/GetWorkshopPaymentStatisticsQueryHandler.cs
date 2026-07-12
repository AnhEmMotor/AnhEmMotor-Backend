using Application.Common.Models;
using Application.Interfaces.Repositories.WorkshopPayment;
using MediatR;

namespace Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStatistics;

public record GetWorkshopPaymentStatisticsQuery : IRequest<Result<WorkshopPaymentStatisticsResponse>>;

public class GetWorkshopPaymentStatisticsQueryHandler(IWorkshopPaymentReadRepository repo) : IRequestHandler<GetWorkshopPaymentStatisticsQuery, Result<WorkshopPaymentStatisticsResponse>>
{
    public async Task<Result<WorkshopPaymentStatisticsResponse>> Handle(
        GetWorkshopPaymentStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var unpaid = payments.Where(p => p.PaymentStatus == "Unpaid").ToList();
        var partial = payments.Where(p => p.PaymentStatus == "Partial").ToList();
        var paidToday = payments.Where(p => p.PaymentStatus == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value >= today)
            .ToList();
        var monthPaid = payments.Where(
            p => p.PaymentStatus == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value >= monthStart)
            .ToList();
        var response = new WorkshopPaymentStatisticsResponse
        {
            Unpaid = unpaid.Count,
            UnpaidAmount = unpaid.Sum(p => p.TotalAmount),
            Partial = partial.Count,
            PartialAmount = partial.Sum(p => p.TotalAmount),
            PaidToday = paidToday.Count,
            PaidTodayAmount = paidToday.Sum(p => p.TotalAmount),
            MonthRevenue = monthPaid.Sum(p => p.TotalAmount),
        };
        return Result<WorkshopPaymentStatisticsResponse>.Success(response);
    }
}
