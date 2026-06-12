using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.RepairOrder;
using Application.Interfaces.Repositories.Vehicle;
using Application.ApiContracts.RepairOrder.Responses;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using Sieve.Models;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Application.Features.Vehicles.Queries.GetVehiclePortfolio;

public sealed class GetVehiclePortfolioQueryHandler(
    IVehicleReadRepository vehicleRepository,
    IRepairOrderReadRepository repairOrderRepository) : IRequestHandler<GetVehiclePortfolioQuery, Result<VehiclePortfolioResponse>>
{
    public async Task<Result<VehiclePortfolioResponse>> Handle(
        GetVehiclePortfolioQuery request,
        CancellationToken cancellationToken)
    {
        var query = request.Query.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result<VehiclePortfolioResponse>.Failure("Query cannot be empty.");
        }

        var queryType = NormalizeQueryType(query, request.QueryType);

        // 1. Find the vehicle
        Expression<Func<Vehicle, bool>>? filter = null;
        if (queryType == "phone")
        {
            filter = v => v.Lead!.PhoneNumber.Contains(query);
        }
        else if (queryType == "licensePlate")
        {
            filter = v => v.LicensePlate.Contains(query);
        }
        else if (queryType == "vin")
        {
            filter = v => v.VinNumber.Contains(query);
        }

        var vehicles = await vehicleRepository.GetPagedAsync<VehicleResponse>(
            new SieveModel { Filters = "" },
            DataFetchMode.ActiveOnly,
            filter,
            cancellationToken);

        if (vehicles == null || vehicles.Items == null || !vehicles.Items.Any())
        {
            return Result<VehiclePortfolioResponse>.Failure("No vehicle found matching the query.");
        }

        var vehicle = vehicles.Items.First();

        // 2. Fetch Repair History
        var roFilter = new SieveModel
        {
            Sorts = $"-{nameof(RepairOrder.CreatedAt)}",
            Filters = $"VehicleId={vehicle.Id}"
        };

        var historyResult = await repairOrderRepository.GetPagedAsync<RepairOrderResponse>(
            roFilter,
            DataFetchMode.ActiveOnly,
            null,
            cancellationToken);

        // 3. Generate Alerts
        var alerts = GenerateAlerts(historyResult?.Items);

        return Result<VehiclePortfolioResponse>.Success(new VehiclePortfolioResponse
        {
            Vehicle = vehicle,
            History = historyResult?.Items ?? new List<RepairOrderResponse>(),
            TotalHistoryCount = (int)(historyResult?.TotalCount ?? 0),
            Alerts = alerts
        });
    }

    private static string NormalizeQueryType(string q, string forcedType)
    {
        if (forcedType != "auto") return forcedType;

        var sanitized = q.Replace(" ", "");
        if (Regex.IsMatch(sanitized, @"^\d{10,}$")) return "phone";
        if (Regex.IsMatch(q, @"^[A-HJ-NPR-Z0-9]{8,}$", RegexOptions.IgnoreCase)) return "vin";
        if (Regex.IsMatch(q, @"^[0-9]{2}[-A-Za-z]?[0-9A-Za-z.-]")) return "licensePlate";

        return "vin";
    }

    private static List<MaintenanceAlertResponse> GenerateAlerts(List<RepairOrderResponse>? history)
    {
        var alerts = new List<MaintenanceAlertResponse>();
        if (history == null || !history.Any()) return alerts;

        var latest = history.First();
        long mileage = (long)latest.Mileage;

        if (mileage >= 3000)
        {
            alerts.Add(new MaintenanceAlertResponse
            {
                Title = "Nhắc bảo dưỡng định kỳ (mốc ODO)",
                Severity = "Cảnh báo",
                Type = mileage >= 5000 ? "warning" : "info",
                Description = "Hiện trạng ODO đang vượt mốc nhắc nhở theo chu kỳ gần nhất."
            });
        }

        alerts.Add(new MaintenanceAlertResponse
        {
            Title = "Nhắc thay phụ tùng theo chu kỳ",
            Severity = "Gợi ý",
            Type = "warning",
            Description = "Có thể cần kiểm tra/đề xuất thay thế phụ tùng theo tình trạng thực tế và ODO."
        });

        return alerts;
    }
}
