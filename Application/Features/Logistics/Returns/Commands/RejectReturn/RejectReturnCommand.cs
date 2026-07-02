using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Enums;
using MediatR;
using System;

namespace Application.Features.Logistics.Returns.Commands.RejectReturn;

public class RejectReturnCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string? RejectionReason { get; set; }
    public string? ProcessedBy { get; set; }
}
