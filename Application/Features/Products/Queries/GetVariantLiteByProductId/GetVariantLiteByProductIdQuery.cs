using Application.ApiContracts.Product.Common;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed record GetVariantLiteByProductIdQuery(int ProductId, bool IncludeDeleted) : IRequest<(List<ProductVariantLiteResponse>? Data, ErrorResponse? Error)>;
