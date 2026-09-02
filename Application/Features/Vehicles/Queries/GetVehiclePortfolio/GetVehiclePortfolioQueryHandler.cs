using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Vehicle;
using Domain.Constants;
using MediatR;
using System.Text.Json;
using MaintenanceHistoryEntity = Domain.Entities.MaintenanceHistory;

namespace Application.Features.Vehicles.Queries.GetVehiclePortfolio;

public class GetVehiclePortfolioQueryHandler(
    IVehicleReadRepository vehicleRepo,
    IMaintenanceHistoryReadRepository maintenanceRepo) : IRequestHandler<GetVehiclePortfolioQuery, Result<VehiclePortfolioResponse?>>
{
    public async Task<Result<VehiclePortfolioResponse?>> Handle(GetVehiclePortfolioQuery req, CancellationToken ct)
    {
        var q = req.Query.Trim();
        if (string.IsNullOrEmpty(q))
            return Result<VehiclePortfolioResponse?>.Failure([Error.Validation("Query cannot be empty.")]);
        var vehicle = await vehicleRepo.GetVehicleForPortfolioAsync(q, req.QueryType, ct).ConfigureAwait(false);
        if (vehicle is null)
            return Result<VehiclePortfolioResponse?>.Success(
                new VehiclePortfolioResponse
                {
                    Vehicle = null!,
                    History = new List<VehiclePortfolioHistoryItem>(),
                    TotalHistoryCount = 0
                });
        var allHistory = await maintenanceRepo.GetByVehicleIdAsync(vehicle.Id, ct, DataFetchMode.All)
            .ConfigureAwait(false);
        List<MaintenanceHistoryEntity> historyItems = req.PageSize > 0 && req.Page > 0
            ? allHistory
				.OrderByDescending(h => h.MaintenanceDate)
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToList()
            : allHistory.ToList();
        int totalHistoryCount = allHistory.Count();
        var vehicleResponse = new VehicleResponse
        {
            Id = vehicle.Id,
            FullName = vehicle.Lead?.FullName ?? string.Empty,
            PhoneNumber = vehicle.Lead?.PhoneNumber ?? string.Empty,
            VinNumber = vehicle.VinNumber,
            EngineNumber = vehicle.EngineNumber,
            LicensePlate = vehicle.LicensePlate,
            PurchaseDate = vehicle.PurchaseDate,
            LeadId = vehicle.LeadId ?? 0,
            ProductVariantId = vehicle.ProductVariantId,
            ProductVariantColorId = vehicle.ProductVariantColorId,
            VariantName = vehicle.ProductVariant?.VariantName,
            ColorName = vehicle.ProductVariantColor?.ColorName,
            ProductName = vehicle.Product?.Name,
            CategoryName = vehicle.Product?.ProductCategory?.Name,
            BrandName = vehicle.Product != null && vehicle.Product.Brand != null ? vehicle.Product.Brand.Name : null,
            WarrantyPeriod = vehicle.Product?.WarrantyPeriod,
            IsActive = vehicle.IsActive,
            Documents = new()
        };
        var historyResponse = historyItems.Select(
            h =>
            {
                List<PortfolioPartItem> details = new();
                try
                {
                    if (!string.IsNullOrWhiteSpace(h.PartsJson))
                    {
                        if (h.PartsJson.TrimStart().StartsWith("["))
                        {
                            var parsedList = JsonSerializer.Deserialize<List<MaintenanceHistoryItemDto>>(h.PartsJson);
                            if (parsedList != null)
                            {
                                foreach (var p in parsedList)
                                {
                                    details.Add(
                                        new PortfolioPartItem
                                        {
                                            Type = p.Type == "Product" ? "Part" : "Service",
                                            VariantName = p.Name,
                                            ProductCode = null,
                                            Count = p.Count
                                        });
                                }
                            }
                        }
                        else if (h.PartsJson.TrimStart().StartsWith("{"))
                        {
                            var oldFormat = JsonSerializer.Deserialize<OldMaintenancePartsJsonDto>(h.PartsJson);
                            if (oldFormat?.Parts != null)
                            {
                                foreach (var p in oldFormat.Parts)
                                {
                                    details.Add(
                                        new PortfolioPartItem
                                        {
                                            Type = "Part",
                                            VariantName = p.ProductVariantName ?? $"Sản phẩm ID: {p.ProductVariantId}",
                                            ProductCode = null,
                                            Count = p.Count
                                        });
                                }
                            }
                            if (oldFormat?.Services != null)
                            {
                                foreach (var s in oldFormat.Services)
                                {
                                    details.Add(
                                        new PortfolioPartItem
                                        {
                                            Type = "Service",
                                            VariantName = s.ServiceName ?? $"Dịch vụ ID: {s.ServiceId}",
                                            ProductCode = null,
                                            Count = s.Count
                                        });
                                }
                            }
                        }
                    }
                } catch
                {
                    details = new();
                }
                return new VehiclePortfolioHistoryItem
                {
                    Id = h.Id,
                    MaintenanceNumber = h.MaintenanceNumber,
                    VehicleId = h.VehicleId,
                    VehicleInfo = null,
                    MaintenanceDate = h.MaintenanceDate,
                    Description = h.Description,
                    Mileage = h.Mileage,
                    TechnicianName = null,
                    PartsCost = h.PartsCost,
                    LaborCost = h.LaborCost,
                    TotalCost = h.TotalCost,
                    NextMaintenanceDate = h.NextMaintenanceDate,
                    NextMaintenanceOdo = h.NextMaintenanceOdo,
                    CreatedAt = h.CreatedAt.GetValueOrDefault(),
                    Status = "Completed",
                    PartsJson = h.PartsJson,
                    Details = details
                };
            })
            .ToList();
        var result = new VehiclePortfolioResponse
        {
            Vehicle = vehicleResponse,
            History = historyResponse,
            TotalHistoryCount = totalHistoryCount
        };
        return Result<VehiclePortfolioResponse?>.Success(result);
    }
}

public class MaintenanceHistoryItemDto
{
    public string? Type { get; set; }

    public string? Name { get; set; }

    public int Count { get; set; }
}

public class OldMaintenancePartsJsonDto
{
    public List<OldPartItem>? Parts { get; set; }
    public List<OldServiceItem>? Services { get; set; }
}

public class OldPartItem
{
    public int ProductVariantId { get; set; }
    public string? ProductVariantName { get; set; }
    public int Count { get; set; }
}

public class OldServiceItem
{
    public int ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int Count { get; set; }
}


