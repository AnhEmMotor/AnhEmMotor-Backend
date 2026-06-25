using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Service;
using Application.Interfaces.Repositories.ServiceCategory;
using Mapster;
using MediatR;
using ServiceEntity = Domain.Entities.Service;

namespace Application.ApiContracts.Service.Responses;

public class CreateServiceCommandHandler(
    IUnitOfWork unitOfWork,
    IServiceReadRepository serviceReadRepository,
    IServiceInsertRepository serviceInsertRepository,
    IServiceCategoryReadRepository categoryRepository) : IRequestHandler<CreateServiceCommand, Result<ServiceResponse>>
{
    public async Task<Result<ServiceResponse>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
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
        var service = new ServiceEntity
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
