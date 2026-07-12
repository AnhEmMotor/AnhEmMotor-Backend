using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Commands;

public record DeleteRepairOrderCommand(int Id) : IRequest<Result>;
