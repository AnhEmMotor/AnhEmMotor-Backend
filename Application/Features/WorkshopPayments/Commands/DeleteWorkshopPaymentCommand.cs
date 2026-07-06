using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WorkshopPayments.Commands;

public record DeleteWorkshopPaymentCommand(int Id) : IRequest<Result>;
