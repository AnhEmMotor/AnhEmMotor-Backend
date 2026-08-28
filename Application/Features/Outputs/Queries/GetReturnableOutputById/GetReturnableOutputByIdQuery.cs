using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetReturnableOutputById;

/// <summary>
/// Query lấy chi tiết đơn hàng chỉ bao gồm các sản phẩm đủ điều kiện hoàn trả (không phải xe máy và chưa gán VIN).
/// </summary>
/// <param name="Id">Mã định danh đơn hàng.</param>
public sealed record GetReturnableOutputByIdQuery(int Id) : IRequest<Result<OrderDetailResponse>>;
