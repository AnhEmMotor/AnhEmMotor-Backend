using Application.Common.Models;
using Application.Interfaces.Repositories.SupplierDebt;
using MediatR;
using System.Linq;

namespace Application.Features.DebtPayments.Queries.GetDebtLogProofImages;

public class GetDebtLogProofImagesQueryHandler(ISupplierDebtReadRepository supplierDebtReadRepository) : IRequestHandler<GetDebtLogProofImagesQuery, Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(
        GetDebtLogProofImagesQuery request,
        CancellationToken cancellationToken)
    {
        var images = await supplierDebtReadRepository.GetDebtLogProofImagesAsync(request.DebtLogId, cancellationToken);
        var urls = images.Select(img => img.ImageUrl).ToList();
        return Result<List<string>>.Success(urls);
    }
}
