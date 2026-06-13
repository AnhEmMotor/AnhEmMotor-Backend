using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Services.Commands;

/// <summary>
/// Lệnh yêu cầu cập nhật thông tin một dịch vụ hiện có.
/// </summary>
public class UpdateServiceCommand : IRequest<Result<ServiceResponse>>
{
    /// <summary>
    /// Mã định danh của dịch vụ cần cập nhật.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên mới của dịch vụ.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Mô tả chi tiết dịch vụ.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá cơ bản của dịch vụ.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Thời gian hoàn thành dự kiến (phút).
    /// </summary>
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Mã danh mục dịch vụ.
    /// </summary>
    public int CategoryId { get; set; }
}