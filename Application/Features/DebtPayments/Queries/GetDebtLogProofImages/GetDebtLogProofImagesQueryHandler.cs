using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;
using System.Linq;

namespace Application.Features.DebtPayments.Queries.GetDebtLogProofImages;

public class GetDebtLogProofImagesQueryHandler(
    ISupplierDebtReadRepository supplierDebtReadRepository) : IRequestHandler<GetDebtLogProofImagesQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(
        GetDebtLogProofImagesQuery request,
        CancellationToken cancellationToken)
    {
        // First get the debt log from the repository? 
        // Wait, supplierDebtReadRepository doesn't have a method specifically for this yet.
        // I will add a method to get images by debt log id.
        var images = await supplierDebtReadRepository.GetDebtLogProofImagesAsync(request.DebtLogId, cancellationToken);
        var urls = images.Select(img => img.ImageUrl).ToList();
        
        return Result<List<string>>.Success(urls);
    }
}
