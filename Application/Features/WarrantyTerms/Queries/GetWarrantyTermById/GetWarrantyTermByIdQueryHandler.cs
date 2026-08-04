using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyTerm;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermById;

public class GetWarrantyTermByIdQueryHandler(IWarrantyTermReadRepository readRepository) : IRequestHandler<GetWarrantyTermByIdQuery, Result<WarrantyTermResponse?>>
{
    public async Task<Result<WarrantyTermResponse?>> Handle(
        GetWarrantyTermByIdQuery request,
        CancellationToken cancellationToken)
    {
        var term = await readRepository.GetByIdAsync(request.Id, cancellationToken, includeBrand: true)
            .ConfigureAwait(false);
        if (term == null)
        {
            return Result<WarrantyTermResponse?>.Failure(
                Error.NotFound($"Warranty term with Id {request.Id} not found.", "Id"));
        }
        var response = new WarrantyTermResponse
        {
            Id = term.Id,
            BrandId = term.BrandId,
            BrandName = term.Brand?.Name ?? string.Empty,
            TermName = term.TermName,
            TermNameJson = term.TermNameJson ?? string.Empty,
            VehicleType = term.VehicleType,
            ErrorCategory = term.ErrorCategory,
            Description = term.Description,
            DescriptionJson = term.DescriptionJson ?? string.Empty,
            DurationMonths = term.DurationMonths,
            DurationKm = term.DurationKm,
            Coverage = term.Coverage,
            Status = term.Status,
            EffectiveDate = term.EffectiveDate,
            ExpirationDate = term.ExpirationDate,
            MediaUrl = term.MediaUrl
        };
        return Result<WarrantyTermResponse?>.Success(response);
    }
}
