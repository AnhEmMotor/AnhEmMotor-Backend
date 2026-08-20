using Application.Common.Models;
using Application.Interfaces.Repositories.Product;
using MediatR;

namespace Application.Features.Marketing.Queries.GetVisitorTracking;

public record GetVisitorTrackingQuery(int Take = 100) : IRequest<Result<List<DetailedProductView>>>;
