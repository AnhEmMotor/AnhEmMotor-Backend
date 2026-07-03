using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.WorkshopPayments.Commands.CreateWorkshopPayment;

public class CreateWorkshopPaymentCommand : IRequest<Result<int>>
{
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? VehicleInfo { get; set; }
    public string? ServiceDescription { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? PaymentStatus { get; set; }
    public string? Notes { get; set; }
}
