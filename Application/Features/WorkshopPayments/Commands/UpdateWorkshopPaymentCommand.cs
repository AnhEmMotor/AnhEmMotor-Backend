using Application.Common.Models;
using MediatR;

namespace Application.Features.WorkshopPayments.Commands;

public record UpdateWorkshopPaymentCommand(
    int Id,
    string? CustomerName,
    string? CustomerPhone,
    string? VehicleInfo,
    string? ServiceDescription,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TotalAmount,
    string PaymentMethod,
    string PaymentStatus,
    DateTimeOffset? PaidAt,
    string? Notes
) : IRequest<Result>;
