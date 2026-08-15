using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Sales.Returns.Commands.CreateReturnRequest;

public class CreateReturnRequestCommand : IRequest<Result<ReturnRequestResponse>>
{
    public int OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Type { get; set; } = "return";
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string OriginalTrackingNumber { get; set; } = string.Empty;
    public List<ReturnRequestItemDto> Items { get; set; } = new();
    public List<IFormFile>? EvidenceImages { get; set; }
}

public class ReturnRequestItemDto
{
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int? ColorId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Reason { get; set; }
}
