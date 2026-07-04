using Application.Features.Logistics.Queries.GetLogisticsDashboard;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Logistics.Queries.GetLogisticsDashboard;

public class GetLogisticsDashboardQueryHandler(
	IParcelDeliveryOrderReadRepository parcelRepo) : IRequestHandler<GetLogisticsDashboardQuery, LogisticsDashboardResponse>
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

		var parcels = await parcelRepo.GetAllAsync(cancellationToken);
		var filtered = parcels
			.Where(x => x.CreatedAt >= from)
			.ToList();

		var response = new LogisticsDashboardResponse();

		var pending = filtered.Count(x => x.Status == ParcelDeliveryStatus.Pending);
		var packing = filtered.Count(x => x.Status == ParcelDeliveryStatus.Packing);
		response.Summary.FulfillmentWorkload = pending + packing;
		response.Summary.FulfillmentWorkloadIsOverload = pending + packing > 50;

		var shipping = filtered.Where(x => x.Status == ParcelDeliveryStatus.Shipping);
		response.Summary.PendingUnreconciledCod = shipping.Sum(x => x.CodAmount);

		var delivered = filtered
			.Where(x => x.Status == ParcelDeliveryStatus.Completed)
			.ToList();
		var otifCount = delivered.Count(x =>
			x.DeliveredAt != null && x.ExpectedAt != null && x.DeliveredAt <= x.ExpectedAt);
		response.Summary.OtifRate = delivered.Count > 0
			? (double)otifCount / delivered.Count
			: 0.0;

		var returned = filtered.Count(x => x.Status == ParcelDeliveryStatus.Returned);
		response.Summary.ReturnsClaimsRate = filtered.Count > 0
			? (double)returned / filtered.Count
			: 0.0;

		response.FulfillmentFunnel = filtered
			.GroupBy(x => x.Status)
			.ToDictionary(g => g.Key.ToString(), g => g.Count());

		response.Trends = filtered
			.Where(x => x.Status == ParcelDeliveryStatus.Completed)
			.GroupBy(x => x.DeliveredAt?.Date)
			.Select(g => new ApiContracts.Logistics.Responses.LogisticsTrendPointResponse
			{
				DayLabel = g.Key?.ToString("dd/MM") ?? "-",
				DeliveredCount = g.Count(),
				ShippingCost = filtered
					.Where(x => x.DeliveredAt?.Date == g.Key)
					.Sum(x => x.ShippingCost)
			})
			.OrderBy(t => t.DayLabel)
			.Take(14)
			.ToList();

		response.CarrierScorecard = filtered
			.Where(x => x.Status == ParcelDeliveryStatus.Completed)
			.GroupBy(x => x.Carrier)
			.Select(g => new ApiContracts.Logistics.Responses.CarrierScoreRowResponse
			{
				Carrier = g.Key,
				DeliveredCount = g.Count(),
				AvgDeliveryDays = g.Average(x => (x.DeliveredAt!.Value - x.CreatedAt).TotalDays),
				AvgShippingCostPerOrder = g.Average(x => x.ShippingCost),
				ReturnsRatio = filtered.Count(x =>
						string.Equals(x.Carrier, g.Key) && x.Status == ParcelDeliveryStatus.Returned)
					/ (double)filtered.Count(x => string.Equals(x.Carrier, g.Key))
			})
			.OrderByDescending(c => c.DeliveredCount)
			.ToList();

		response.Exceptions = new List<ApiContracts.Logistics.Responses.LogisticsExceptionRowResponse>();

		response.Exceptions.AddRange(
			filtered
				.Where(x => x.Status == ParcelDeliveryStatus.Pending && (now - x.CreatedAt).TotalHours > 24)
				.Take(20)
				.Select(x => new ApiContracts.Logistics.Responses.LogisticsExceptionRowResponse
				{
					Type = "ngam_kho",
					TrackingNumber = x.TrackingNumber,
					Message = "Đơn pending quá 24h mà chưa chuyển trạng thái đóng gói.",
					CreatedAt = x.CreatedAt
				}));

		response.Exceptions.AddRange(
			filtered
				.Where(x => x.Status == ParcelDeliveryStatus.Shipping && (now - x.CreatedAt).TotalDays > 4)
				.Take(20)
				.Select(x => new ApiContracts.Logistics.Responses.LogisticsExceptionRowResponse
				{
					Type = "giao_cham",
					TrackingNumber = x.TrackingNumber,
					Message = "Đơn đang shipping quá 4 ngày chưa cập nhật Completed.",
					CreatedAt = x.CreatedAt
				}));

		response.Exceptions.AddRange(
			filtered
				.Where(x => x.Status == ParcelDeliveryStatus.Returned && x.InspectedAt == null)
				.Take(20)
				.Select(x => new ApiContracts.Logistics.Responses.LogisticsExceptionRowResponse
				{
					Type = "hoan_cho_kiem_tra",
					TrackingNumber = x.TrackingNumber,
					Message = "Hàng hoàn đã về nhưng chưa khui hộp/duyệt nhập lại kho.",
					CreatedAt = x.CreatedAt
				}));

		return response;
	}
}
