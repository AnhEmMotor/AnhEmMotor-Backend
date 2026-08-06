using Application.ApiContracts.Admin.Warranty;
using Application.Common.Models;
using Application.Interfaces.Repositories.WarrantyTerm;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyTerms.Queries.GetWarrantyTermsList;

public class GetWarrantyTermsListQueryHandler(IWarrantyTermReadRepository readRepository) : IRequestHandler<GetWarrantyTermsListQuery, Result<PagedResult<WarrantyTermResponse>>>
{
    public async Task<Result<PagedResult<WarrantyTermResponse>>> Handle(
        GetWarrantyTermsListQuery request,
        CancellationToken cancellationToken)
    {
        var terms = await readRepository.GetAllAsync(cancellationToken, includeBrand: true).ConfigureAwait(false);
        var totalCount = terms.Count();
        var query = terms.AsQueryable();
        query = query.OrderByDescending(t => t.Id);
        if (!string.IsNullOrWhiteSpace(request.SieveModel?.Filters))
        {
            var filterParts = request.SieveModel.Filters
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in filterParts)
            {
                var segments = part.Split("==", 2);
                if (segments.Length == 2)
                {
                    var field = segments[0].Trim().ToLowerInvariant();
                    var value = segments[1].Trim();
                    if (field == "brandid" && int.TryParse(value, out var brandId))
                    {
                        query = query.Where(t => t.BrandId == brandId);
                    } else if (field == "status")
                    {
                        query = query.Where(t => t.Status == value);
                    } else if (field == "searchterm")
                    {
                        query = query.Where(
                            t => t.TermName.Contains(value) ||
                                (t.ErrorCategory != null && t.ErrorCategory.Contains(value)) ||
                                (t.Description != null && t.Description.Contains(value)));
                    }
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(request.SieveModel?.Sorts))
        {
            var sortParts = request.SieveModel.Sorts
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in sortParts)
            {
                var isDesc = part.StartsWith("-");
                var field = (isDesc ? part[1..] : part).ToLowerInvariant().Trim();
                query = field switch
                {
                    "id" => isDesc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
                    "brandid" => isDesc ? query.OrderByDescending(t => t.BrandId) : query.OrderBy(t => t.BrandId),
                    "termname" => isDesc ? query.OrderByDescending(t => t.TermName) : query.OrderBy(t => t.TermName),
                    "status" => isDesc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                    "durationmonths" => isDesc
                        ? query.OrderByDescending(t => t.DurationMonths)
                        : query.OrderBy(t => t.DurationMonths),
                    "effectivedate" => isDesc
                        ? query.OrderByDescending(t => t.EffectiveDate)
                        : query.OrderBy(t => t.EffectiveDate),
                    _ => query
                };
            }
        } else
        {
            query = query.OrderByDescending(t => t.Id);
        }
        var page = request.SieveModel?.Page ?? 1;
        var pageSize = request.SieveModel?.PageSize ?? 10;
        var pagedResult = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var responses = pagedResult.Select(
            t => new WarrantyTermResponse
            {
                Id = t.Id,
                BrandId = t.BrandId,
                BrandName = t.Brand?.Name ?? string.Empty,
                TermName = t.TermName,
                TermNameJson = t.TermNameJson ?? string.Empty,
                VehicleType = t.VehicleType,
                ErrorCategory = t.ErrorCategory,
                Description = t.Description,
                DescriptionJson = t.DescriptionJson ?? string.Empty,
                DurationMonths = t.DurationMonths,
                DurationKm = t.DurationKm,
                Coverage = t.Coverage,
                Status = t.Status,
                EffectiveDate = t.EffectiveDate,
                ExpirationDate = t.ExpirationDate,
                MediaUrl = t.MediaUrl
            })
            .ToList();
        return Result<PagedResult<WarrantyTermResponse>>.Success(
            new PagedResult<WarrantyTermResponse>(responses, totalCount, page, pageSize));
    }
}
