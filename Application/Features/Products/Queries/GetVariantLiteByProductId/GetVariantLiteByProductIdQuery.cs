using Application.ApiContracts.Product.Common;
using MediatR;

namespace Application.Features.Products.Queries.GetVariantLiteByProductId;

public sealed record GetVariantLiteByProductIdQuery(int ProductId, bool IncludeDeleted) : IRequest<(List<ProductVariantLiteResponse>? Data, Common.Models.ErrorResponse? Error)>;
