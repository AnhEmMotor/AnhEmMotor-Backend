using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Services.Commands.CreateService;

/// <summary>
/// Lệnh yêu cầu tạo mới một dịch vụ.
/// </summary>
public class CreateServiceCommand : IRequest<Result<ServiceResponse>>
{
    /// <summary>
    /// Tên của dịch vụ.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Mô tả chi tiết về dịch vụ.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Giá cơ bản của dịch vụ.
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Thời gian thực hiện dự kiến (phút).
    /// </summary>
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Mã định danh của danh mục dịch vụ.
    /// </summary>
    public int CategoryId { get; set; }
}
