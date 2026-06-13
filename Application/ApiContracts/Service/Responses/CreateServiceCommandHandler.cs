using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Service;
using Application.Interfaces.Repositories.ServiceCategory;
using Domain.Entities;
using Domain.Primitives;
using Mapster;
using MediatR;

namespace Application.Features.Services.Commands.CreateService;

/// <summary>
/// Xử lý việc tạo mới một dịch vụ trong hệ thống.
/// </summary>
public class CreateServiceCommandHandler (
    IUnitOfWork unitOfWork,
    IServiceReadRepository serviceReadRepository,
    IServiceInsertRepository serviceInsertRepository,
    IServiceCategoryReadRepository categoryRepository) : IRequestHandler<CreateServiceCommand, Result<ServiceResponse>>
{
    /// <summary>
    /// Xử lý logic tạo dịch vụ: kiểm tra danh mục và tính duy nhất của tên.
    /// </summary>
    /// <param name="request">Dữ liệu yêu cầu tạo.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả chứa thông tin dịch vụ vừa tạo.</returns>
    public async Task<Result<ServiceResponse>> Handle (
        CreateServiceCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categoryRepository
            .GetByIdAsync(request.CategoryId, cancellationToken)
            .ConfigureAwait(false);

        if (category is null)
        {
            return Error.NotFound("Không tìm thấy danh mục dịch vụ được yêu cầu.", nameof(request.CategoryId));
        }

        var isDuplicateName = await serviceReadRepository
            .NameExistsAsync(request.Name, cancellationToken)
            .ConfigureAwait(false);

        if (isDuplicateName)
        {
            return Error.BadRequest("Tên dịch vụ đã tồn tại trong hệ thống.", nameof(request.Name));
        }

        var service = new Service
        {
            Name = request.Name,
            Description = request.Description,
            BasePrice = request.BasePrice,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            CategoryId = request.CategoryId
        };

        serviceInsertRepository.Add(service);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return service.Adapt<ServiceResponse>();
    }
}
