using Application.ApiContracts.Service.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Service;
using Application.Interfaces.Repositories.ServiceCategory;
using Mapster;
using MediatR;

namespace Application.Features.Services.Commands;

public class UpdateServiceCommandHandler(
    IUnitOfWork unitOfWork,
    IServiceReadRepository serviceReadRepository,
    IServiceUpdateRepository serviceUpdateRepository,
    IServiceCategoryReadRepository categoryRepository) : IRequestHandler<UpdateServiceCommand, Result<ServiceResponse>>
{
    public async Task<Result<ServiceResponse>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var service = await serviceReadRepository
            .GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (service is null)
        {
            return Error.NotFound("Dịch vụ không tồn tại.", nameof(request.Id));
        }
        var category = await categoryRepository
            .GetByIdAsync(request.CategoryId, cancellationToken)
            .ConfigureAwait(false);
        if (category is null)
        {
            return Error.NotFound("Danh mục dịch vụ không hợp lệ.", nameof(request.CategoryId));
        }
        var isDuplicateName = await serviceReadRepository
            .NameExistsExcludingAsync(request.Name, request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (isDuplicateName)
        {
            return Error.BadRequest("Tên dịch vụ đã tồn tại.", nameof(request.Name));
        }
        request.Adapt(service);
        serviceUpdateRepository.Update(service);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return service.Adapt<ServiceResponse>();
    }
}
