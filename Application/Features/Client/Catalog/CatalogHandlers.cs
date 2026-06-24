using MediatR;
using Application.ApiContracts.Client.Catalog;
using Application.Interfaces.Repositories.Lead.Lead;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Application.Features.Client.Catalog
{
    public record GetProductsQuery(string Search, int? CategoryId) : IRequest<List<ProductSummaryResponse>>;
    public record GetProductDetailQuery(int Id) : IRequest<ProductDetailResponse>;
    public record RequestConsultationCommand(ConsultationRequest Request) : IRequest<bool>;

    public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<ProductSummaryResponse>>
    {
        // Use a Generic Product Repository if available, otherwise mock
        public async Task<List<ProductSummaryResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new List<ProductSummaryResponse>
            {
                new ProductSummaryResponse(1, "Honda SH", "url", 100000000, "Ưu đãi 2%"),
                new ProductSummaryResponse(2, "Honda Vision", "url", 30000000, "Tặng mũ bảo hiểm")
            });
        }
    }

    public class GetProductDetailHandler : IRequestHandler<GetProductDetailQuery, ProductDetailResponse>
    {
        public async Task<ProductDetailResponse> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new ProductDetailResponse(
                request.Id, "Honda SH", "Mô tả chi tiết xe SH", 100000000, 
                new List<string> { "Phanh ABS", "Smartkey" }, 
                true, "Tương thích hoàn toàn"));
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
