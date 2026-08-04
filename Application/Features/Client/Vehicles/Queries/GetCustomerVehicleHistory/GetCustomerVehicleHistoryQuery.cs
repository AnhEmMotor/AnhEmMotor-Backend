using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Client.Vehicles.Queries.GetCustomerVehicleHistory;

public class CustomerVehicleHistoryResponse
{
    public List<PurchaseHistoryDto> PurchaseHistory { get; set; } = new();
    public List<WarrantyHistoryDto> WarrantyHistory { get; set; } = new();
}

public class PurchaseHistoryDto
{
    public int Id { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class WarrantyHistoryDto
{
    public int Id { get; set; }
    public DateTime? StartDate { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? CoverageAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GetCustomerVehicleHistoryQuery : IRequest<Result<CustomerVehicleHistoryResponse>>
{
    public Guid UserId { get; set; }
    public int VehicleId { get; set; }
}
