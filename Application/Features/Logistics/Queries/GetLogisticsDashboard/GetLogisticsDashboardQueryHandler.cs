using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Enums;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class GetLogisticsDashboardQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository) : IRequestHandler<GetLogisticsDashboardQuery, LogisticsDashboardResponse>
{
    public async Task<LogisticsDashboardResponse> Handle(
        GetLogisticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        DateTime from = request.Range switch
        {
            "month" => now.AddDays(-30),
            "year" => now.AddDays(-365),
            _ => now.AddDays(-1),
        };
        var parcels = (await parcelDeliveryOrderReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .Where(x => x.CreatedAt >= from)
            .ToList();
        var response = new LogisticsDashboardResponse();
        var pending = parcels.Count(x => x.Status == ParcelDeliveryStatus.Pending);
        var packing = parcels.Count(x => x.Status == ParcelDeliveryStatus.Packing);
        response.Summary.FulfillmentWorkload = pending + packing;
        response.Summary.FulfillmentWorkloadIsOverload = response.Summary.FulfillmentWorkload > 50;
        var shipping = parcels.Where(x => x.Status == ParcelDeliveryStatus.Shipping);
        response.Summary.PendingUnreconciledCod = shipping.Sum(x => x.CodAmount);
        var delivered = parcels.Where(x => x.Status == ParcelDeliveryStatus.Completed).ToList();
        var otif = delivered.Count > 0
            ? delivered.Count(x => x.DeliveredAt != null && x.ExpectedAt != null && x.DeliveredAt <= x.ExpectedAt)
            : 0;
        response.Summary.OtifRate = delivered.Count > 0 ? (double)otif / delivered.Count : 0;
        var returned = parcels.Count(x => x.Status == ParcelDeliveryStatus.Returned);
        var totalSent = parcels.Count;
        response.Summary.ReturnsClaimsRate = totalSent > 0 ? (double)returned / totalSent : 0;
        response.FulfillmentFunnel = parcels
            .GroupBy(x => x.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());
        response.Trends = [.. parcels
            .Where(x => x.Status == ParcelDeliveryStatus.Completed)
            .GroupBy(x => x.DeliveredAt?.Date)
            .Select(
                g => new LogisticsTrendPointResponse
                {
                    DayLabel = g.Key?.ToString("dd/MM") ?? "-",
                    DeliveredCount = g.Count(),
                    ShippingCost = parcels.Where(x => x.DeliveredAt?.Date == g.Key).Sum(x => x.ShippingCost)
                })
            .OrderBy(x => x.DayLabel)
            .Take(14)];
        response.CarrierScorecard = [.. parcels
            .Where(x => x.Status == ParcelDeliveryStatus.Completed)
            .GroupBy(x => x.Carrier)
            .Select(
                g => new CarrierScoreRowResponse
                {
                    Carrier = g.Key,
                    DeliveredCount = g.Count(),
                    AvgDeliveryDays = g.Average(x => (x.DeliveredAt!.Value - x.CreatedAt).TotalDays),
                    AvgShippingCostPerOrder = g.Any() ? g.Average(x => x.ShippingCost) : 0,
                    ReturnsRatio =
                        parcels.Count(
                                    x => string.Compare(x.Carrier, g.Key) == 0 &&
                                        x.Status == ParcelDeliveryStatus.Returned) /
                                (double)parcels.Count(x => string.Compare(x.Carrier, g.Key) == 0)
                })
            .OrderByDescending(x => x.DeliveredCount)];
        response.Exceptions = [];
        var ngams = parcels.Where(x => x.Status == ParcelDeliveryStatus.Pending && (now - x.CreatedAt).TotalHours > 24)
            .Take(20);
        response.Exceptions
            .AddRange(
                ngams.Select(
                    x => new LogisticsExceptionRowResponse
                    {
                        Type = "ngam_kho",
                        TrackingNumber = x.TrackingNumber,
                        Message = $"Đơn pending quá 24h mà chưa chuyển trạng thái đóng gói.",
                        CreatedAt = x.CreatedAt
                    }));
        var chams = parcels.Where(x => x.Status == ParcelDeliveryStatus.Shipping && (now - x.CreatedAt).TotalDays > 4)
            .Take(20);
        response.Exceptions
            .AddRange(
                chams.Select(
                    x => new LogisticsExceptionRowResponse
                    {
                        Type = "giao_cham",
                        TrackingNumber = x.TrackingNumber,
                        Message = "Đơn đang shipping quá 4 ngày chưa cập nhật Completed từ 3PL.",
                        CreatedAt = x.CreatedAt
                    }));
        var hoanchams = parcels.Where(x => x.Status == ParcelDeliveryStatus.Returned && x.InspectedAt == null).Take(20);
        response.Exceptions
            .AddRange(
                hoanchams.Select(
                    x => new LogisticsExceptionRowResponse
                    {
                        Type = "hoan_cho_kiem_tra",
                        TrackingNumber = x.TrackingNumber,
                        Message = "Hàng hoàn đã về nhưng chưa khui hộp/duyệt nhập lại kho.",
                        CreatedAt = x.CreatedAt
                    }));
        return response;
    }
}

