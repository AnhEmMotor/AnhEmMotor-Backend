using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Service;
using Domain.Entities;
using Domain.Primitives;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Handler xử lý yêu cầu lấy danh sách dịch vụ có phân trang.
/// </summary>
public sealed class GetServicesListQueryHandler (
    IServiceReadRepository serviceRepository,
    ISieveProcessor sieveProcessor) : IRequestHandler<GetServicesListQuery, Result<PagedResult<ServiceResponse>>>
{
    /// <summary>
    /// Thực hiện truy vấn danh sách dịch vụ, áp dụng bộ lọc Sieve và phân trang.
    /// </summary>
    /// <param name="request">Thông tin phân trang và lọc.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>
    /// Danh sách dịch vụ được phân trang.
    /// </returns>
    public async Task<Result<PagedResult<ServiceResponse>>> Handle (
        GetServicesListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = serviceRepository.GetQueryable().AsNoTracking();

        var filteredQuery = sieveProcessor.Apply(request.SieveModel, query);

        var totalCount = await filteredQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var services = await filteredQuery
            .Skip((request.SieveModel.Page!.Value - 1) * request.SieveModel.PageSize!.Value)
            .Take(request.SieveModel.PageSize.Value)
            .ProjectToType<ServiceResponse>()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<ServiceResponse>(services, totalCount, request.SieveModel.Page.Value, request.SieveModel.PageSize.Value);
    }
}