using Application.ApiContracts.RepairOrder.Responses;
using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.RepairOrder;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Sieve.Models;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Application.Features.Vehicles.Queries.GetVehiclePortfolio;

public partial class GetVehiclePortfolioQueryHandler(
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
        Expression<Func<Vehicle, bool>>? filter = null;
        if (string.Compare(queryType, VehiclePortfolio.QueryTypePhone) == 0)
        {
            filter = v => v.Lead!.PhoneNumber.Contains(query);
        } else if (string.Compare(queryType, VehiclePortfolio.QueryTypeLicensePlate) == 0)
        {
            filter = v => v.LicensePlate.Contains(query);
        } else if (string.Compare(queryType, VehiclePortfolio.QueryTypeVin) == 0)
        {
            filter = v => v.VinNumber.Contains(query);
        }
        var vehicles = await vehicleRepository.GetPagedAsync<VehicleResponse>(
            new SieveModel { Filters = string.Empty },
            DataFetchMode.ActiveOnly,
            filter,
            cancellationToken)
            .ConfigureAwait(false);
        if (vehicles == null || vehicles.Items == null || vehicles.Items.Count == 0)
        {
            return Result<VehiclePortfolioResponse>.Failure("No vehicle found matching the query.");
        }
        var vehicle = vehicles.Items.First();
        var roFilter = new SieveModel
        {
            Sorts = $"-{nameof(RepairOrder.CreatedAt)}",
            Filters = $"VehicleId={vehicle.Id}"
        };
        var historyResult = await repairOrderRepository.GetPagedAsync<RepairOrderResponse>(
            roFilter,
            DataFetchMode.ActiveOnly,
            null,
            cancellationToken)
            .ConfigureAwait(false);
        var alerts = GenerateAlerts(historyResult?.Items);
        return Result<VehiclePortfolioResponse>.Success(
            new VehiclePortfolioResponse
            {
                Vehicle = vehicle,
                History = historyResult?.Items ?? [],
                TotalHistoryCount = (int)(historyResult?.TotalCount ?? 0),
                Alerts = alerts
            });
    }

    private static string NormalizeQueryType(string q, string forcedType)
    {
        if (string.Compare(forcedType, VehiclePortfolio.QueryTypeAuto) != 0)
            return forcedType;
        var sanitized = q.Replace(" ", string.Empty);
        if (PhoneRegex().IsMatch(sanitized))
            return VehiclePortfolio.QueryTypePhone;
        if (VinRegex().IsMatch(q))
            return VehiclePortfolio.QueryTypeVin;
        if (LicensePlateRegex().IsMatch(q))
            return VehiclePortfolio.QueryTypeLicensePlate;
        return VehiclePortfolio.QueryTypeVin;
    }

    private static List<MaintenanceAlertResponse> GenerateAlerts(List<RepairOrderResponse>? history)
    {
        var alerts = new List<MaintenanceAlertResponse>();
        if (history == null || history.Count == 0)
            return alerts;
        var latest = history.First();
        long mileage = latest.Mileage;
        if (mileage >= 3000)
        {
            alerts.Add(
                new MaintenanceAlertResponse
                {
                    Title = "Nhắc bảo dưỡng định kỳ (mốc ODO)",
                    Severity = VehiclePortfolio.AlertSeverityWarning,
                    Type = mileage >= 5000 ? VehiclePortfolio.AlertTypeWarning : VehiclePortfolio.AlertTypeInfo,
                    Description = "Hiện trạng ODO đang vượt mốc nhắc nhở theo chu kỳ gần nhất."
                });
        }
        alerts.Add(
            new MaintenanceAlertResponse
            {
                Title = "Nhắc thay phụ tùng theo chu kỳ",
                Severity = VehiclePortfolio.AlertSeveritySuggestion,
                Type = VehiclePortfolio.AlertTypeWarning,
                Description = "Có thể cần kiểm tra/đề xuất thay thế phụ tùng theo tình trạng thực tế và ODO."
            });
        return alerts;
    }

    [GeneratedRegex(@"^\d{10,}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"^[A-HJ-NPR-Z0-9]{8,}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VinRegex();

    [GeneratedRegex(@"^[0-9]{2}[-A-Za-z]?[0-9A-Za-z.-]")]
    private static partial Regex LicensePlateRegex();
}
