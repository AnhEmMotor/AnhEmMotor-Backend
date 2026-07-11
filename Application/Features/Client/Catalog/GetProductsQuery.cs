using Application.ApiContracts.Client.Catalog;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using System;
using System.Linq;

namespace Application.Features.Client.Catalog
{
    public record GetProductsQuery(string Search, int? CategoryId) : IRequest<List<ProductSummaryResponse>>;

    public record GetProductDetailQuery(int Id) : IRequest<ProductDetailResponse>;

    public record RequestConsultationCommand(ConsultationRequest Request) : IRequest<bool>;

    public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<ProductSummaryResponse>>
    {
        private readonly IProductReadRepository _productReadRepository;

        public GetProductsHandler(IProductReadRepository productReadRepository) => _productReadRepository = productReadRepository;

        public async Task<List<ProductSummaryResponse>> Handle(
            GetProductsQuery request,
            CancellationToken cancellationToken)
        {
            return await _productReadRepository.GetClientCatalogProductsAsync(
                request.Search ?? string.Empty,
                request.CategoryId,
                cancellationToken);
        }
    }

    public class GetProductDetailHandler(IProductReadRepository productReadRepository) : IRequestHandler<GetProductDetailQuery, ProductDetailResponse>
    {
        public async Task<ProductDetailResponse> Handle(
            GetProductDetailQuery request,
            CancellationToken cancellationToken)
        {
            var product = await productReadRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product {request.Id} not found");
            }

            var variant = product.ProductVariants
                .Where(v => v.DeletedAt == null)
                .OrderBy(v => v.Id)
                .FirstOrDefault();

            return new ProductDetailResponse(
                product.Id,
                product.Name ?? "Sản phẩm",
                product.ShortDescription ?? string.Empty,
                variant?.Price ?? 0m,
                new List<string>(),
                false,
                string.Empty);
        }
    }

    public class RequestConsultationHandler : IRequestHandler<RequestConsultationCommand, bool>
    {
        private readonly ILeadInsertRepository _leadRepo;

        public RequestConsultationHandler(ILeadInsertRepository leadRepo) => _leadRepo = leadRepo;

        public async Task<bool> Handle(RequestConsultationCommand request, CancellationToken cancellationToken)
        {
            var lead = new Lead
            {
                InterestedVehicle = request.Request.ProductId.ToString(),
                Notes = request.Request.CustomerNote,
                Source = "Catalog",
                CreatedAt = DateTime.UtcNow,
                Status = "New",
                Priority = "Warm"
            };
            await _leadRepo.AddAsync(lead, cancellationToken);
            return true;
        }
    }
}
