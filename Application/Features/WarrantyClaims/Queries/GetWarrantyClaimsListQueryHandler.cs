using Application.ApiContracts.Admin.Workshop.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyClaims.Queries;

public class GetWarrantyClaimsListQueryHandler(IWarrantyClaimReadRepository readRepo) : IRequestHandler<GetWarrantyClaimsListQuery, Result<PagedResult<WarrantyClaimResponse>>>
{
    public async Task<Result<PagedResult<WarrantyClaimResponse>>> Handle(
        GetWarrantyClaimsListQuery req,
        CancellationToken ct)
    {
        var claims = await readRepo.GetAllWithDetailsAsync(req.Mode, ct).ConfigureAwait(false);
        
        if (req.Sieve != null && !string.IsNullOrWhiteSpace(req.Sieve.Filters))
        {
            var filters = req.Sieve.Filters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var filter in filters)
            {
                if (filter.StartsWith("status=="))
                {
                    if (int.TryParse(filter.Substring(8), out int status))
                    {
                        claims = claims.Where(c => c.Status == status);
                    }
                }
                else if (filter.StartsWith("search=="))
                {
                    var search = filter.Substring(8).ToLower();
                    claims = claims.Where(c => 
                        (c.ClaimNumber != null && c.ClaimNumber.ToLower().Contains(search)) ||
                        (c.Vehicle?.LicensePlate != null && c.Vehicle.LicensePlate.ToLower().Contains(search)) ||
                        (c.Vehicle?.Lead?.FullName != null && c.Vehicle.Lead.FullName.ToLower().Contains(search)) ||
                        (c.Vehicle?.User?.FullName != null && c.Vehicle.User.FullName.ToLower().Contains(search)));
                }
            }
        }

        var totalCount = claims.Count();
        var page = req.Sieve?.Page ?? 1;
        var pageSize = req.Sieve?.PageSize ?? 10;
        IEnumerable<WarrantyClaim> paged;
        if (req.Sieve != null && !string.IsNullOrWhiteSpace(req.Sieve.Sorts))
        {
            paged = ApplySorting(claims, req.Sieve.Sorts!);
        } else
        {
            paged = claims.OrderByDescending(c => c.CreatedAt);
        }
        paged = paged.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var responses = paged.Select(Map).ToList();
        return Result<PagedResult<WarrantyClaimResponse>>.Success(
            new PagedResult<WarrantyClaimResponse>(responses, totalCount, page, pageSize));
    }

    private static IEnumerable<WarrantyClaim> ApplySorting(IEnumerable<WarrantyClaim> query, string sorts)
    {
        var parts = sorts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedEnumerable<WarrantyClaim>? ordered = null;
        bool first = true;
        foreach (var part in parts)
        {
            var isDesc = part.StartsWith("-");
            var field = (isDesc ? part[1..] : part).ToLowerInvariant();
            if (first || ordered == null)
            {
                ordered = field switch
                {
                    "id" => isDesc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                    "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    "status" => isDesc ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
                    _ => query.OrderByDescending(c => c.CreatedAt)
                };
                first = false;
            } else
            {
                ordered = field switch
                {
                    "id" => isDesc ? ordered.ThenByDescending(c => c.Id) : ordered.ThenBy(c => c.Id),
                    "createdat" => isDesc
                        ? ordered.ThenByDescending(c => c.CreatedAt)
                        : ordered.ThenBy(c => c.CreatedAt),
                    "status" => isDesc ? ordered.ThenByDescending(c => c.Status) : ordered.ThenBy(c => c.Status),
                    _ => ordered
                };
            }
        }
        return ordered ?? query.OrderByDescending(c => c.CreatedAt);
    }

    private static WarrantyClaimResponse Map(WarrantyClaim c)
    {
        var vehicle = c.Vehicle;
        var lead = vehicle?.Lead;
        string? vehicleInfo = null;
        if (vehicle != null)
        {
            var parts = new List<string?>();
            if (!string.IsNullOrWhiteSpace(vehicle.VinNumber))
                parts.Add(vehicle.VinNumber);
            if (!string.IsNullOrWhiteSpace(vehicle.EngineNumber))
                parts.Add(vehicle.EngineNumber);
            vehicleInfo = parts.Count > 0 ? string.Join(" | ", parts) : null;
        }
        var user = vehicle?.User;
        var leadAddress = lead?.Address;
        return new WarrantyClaimResponse
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            VehicleId = c.VehicleId,
            VehicleInfo = vehicleInfo,
            VehiclePlate = vehicle?.LicensePlate,
            CustomerName = lead != null ? lead.FullName : user?.FullName,
            CustomerPhone = lead != null ? lead.PhoneNumber : user?.PhoneNumber,
            CustomerAddress = leadAddress,
            StatusText = WarrantyClaimStatus.GetLabel(c.Status),
            VehicleVin = vehicle?.VinNumber,
            IssueDescription = c.IssueDescription,
            MediaUrls = c.MediaUrls,
            ServiceCenterName = c.ServiceCenterName,
            ManufacturerClaimNumber = c.ManufacturerClaimNumber,
            Status = c.Status,
            ManufacturerDecision = c.ManufacturerDecision,
            IsRecall = c.IsRecall,
            TotalPartsCost = c.TotalPartsCost,
            TotalLaborCost = c.TotalLaborCost,
            Parts =
                c.WarrantyClaimParts
                    .Where(p => p.DeletedAt == null)
                    .Select(
                        p => new WarrantyClaimPartResponse
                    {
                        Id = p.Id,
                        WarrantyClaimId = p.WarrantyClaimId,
                        PartName = p.PartName,
                        PartCode = p.PartCode,
                        UnitPrice = p.UnitPrice,
                        Status = p.Status
                    })
                    .ToList(),
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            IsDeleted = c.DeletedAt != null
        };
    }
}
