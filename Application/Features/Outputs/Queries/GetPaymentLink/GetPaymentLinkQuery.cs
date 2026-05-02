using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetPaymentLink;

public sealed record GetPaymentLinkQuery(int OrderId) : IRequest<Result<string>>;

