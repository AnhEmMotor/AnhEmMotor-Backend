using Application.ApiContracts.Payment.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetPaymentLink;

public sealed record GetPaymentLinkQuery(int OrderId, string? CurrentUserId) : IRequest<Result<PaymentLinkResponse>>;
