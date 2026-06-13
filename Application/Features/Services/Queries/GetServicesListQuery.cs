using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Domain.Primitives;
using MediatR;
using Sieve.Models;

namespace Application.Features.Services.Queries;

/// <summary>
/// Truy vấn lấy danh sách dịch vụ hỗ trợ lọc và phân trang qua Sieve.
/// </summary>
public class GetServicesListQuery : IRequest<Result<PagedResult<ServiceResponse>>>
{
    /// <summary>
    /// Model chứa thông tin lọc, sắp xếp và phân trang của Sieve.
    /// </summary>
    public SieveModel SieveModel { get; set; } = new ();
}