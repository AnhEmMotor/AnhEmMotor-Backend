using Application.Common.Models;
using MediatR;

namespace Application.Features.PurchaseRequests.Queries.GetImportPurchaseRequestTemplate;

public sealed record GetImportPurchaseRequestTemplateQuery : IRequest<Result<byte[]>>;
