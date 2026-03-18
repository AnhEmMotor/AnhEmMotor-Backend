using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Queries.GetProductAttributeLabels;

public sealed record GetProductAttributeLabelsQuery : IRequest<Result<Dictionary<string, string>>>;
