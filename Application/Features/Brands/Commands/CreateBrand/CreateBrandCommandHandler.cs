using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Mapster;
using MediatR;
using BrandEntity = Domain.Entities.Brand;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandler(
    IBrandInsertRepository repository,
    IBrandReadRepository brandReadRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBrandCommand, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var cleanName = request.Name?.Trim();
        var cleanDescription = request.Description?.Trim();

        if (cleanName == null)
            return Error.BadRequest("Name is empty/null, please check again");

        var existingBrands = await brandReadRepository.GetByNameAsync(cleanName, cancellationToken);
        if (existingBrands.Any())
        {
            // Trả về lỗi Domain/Business cụ thể, đừng throw exception vô tội vạ
            return Result<BrandResponse>.Failure("Brand name already exists.");
        }

        // 3. Mapping & Entity Creation
        // Đừng dùng Adapt tự động hoàn toàn nếu bạn muốn kiểm soát dữ liệu.
        // Hoặc map xong thì phải ghi đè lại bằng dữ liệu sạch.
        var brand = request.Adapt<BrandEntity>();
        brand.Name = cleanName; // Gán cứng lại bằng tên sạch
        brand.Description = cleanDescription;

        // 4. Persistence
        repository.Add(brand);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return brand.Adapt<BrandResponse>();
    }
}
