using Application.Common.Models;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using Domain.Entities;
using MediatR;

public sealed class DeleteManyBrandsCommandHandler(
    IBrandReadRepository readRepository,
    IBrandDeleteRepository deleteRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteManyBrandsCommand, Result>
{
    public async Task<Result> Handle(
        DeleteManyBrandsCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Sanitize Data (Có thể làm ở đây hoặc Validator, nhưng Distinct ở đây cho chắc logic)
        // Nếu Validator đã check NotEmpty thì không cần check lại Count > 0
        var uniqueIds = request.Ids.Distinct().ToList();

        // 2. Optimized Data Fetching (CHỈ GỌI 1 LẦN DUY NHẤT)
        // Lấy tất cả lên, kể cả đã xóa hay chưa xóa
        var existingBrands = await readRepository.GetByIdAsync(uniqueIds, cancellationToken, DataFetchMode.All)
            .ConfigureAwait(false);

        // Tạo Dictionary để tra cứu O(1), tránh loop lồng loop O(n^2)
        var existingBrandsMap = existingBrands.ToDictionary(b => b.Id);

        var errorDetails = new List<Error>();
        var brandsToDelete = new List<Brand>(); // List chứa các entity hợp lệ để xóa

        // 3. Validation Logic (Check từng ID một)
        foreach (var id in uniqueIds)
        {
            // Case 1: Không tìm thấy trong DB
            if (!existingBrandsMap.TryGetValue(id, out var brand))
            {
                errorDetails.Add(Error.NotFound($"Brand with Id {id} not found.", "Id"));
                continue; // Skip logic dưới
            }

            // Case 2: Tìm thấy nhưng đã bị xóa (Soft Deleted)
            // Giả định bạn có thuộc tính IsDeleted hoặc check DeletedAt != null
            if (brand.DeletedAt != null)
            {
                errorDetails.Add(Error.BadRequest($"Brand with Id {id} has already been deleted.", "Id"));
                continue;
            }

            // Case 3: Hợp lệ -> Thêm vào danh sách chờ xử trảm
            brandsToDelete.Add(brand);
        }

        // 4. Return Errors if any (Fail Fast strategy)
        // Nếu có bất kỳ lỗi nào, trả về lỗi ngay, KHÔNG thực hiện xóa dở dang (Partial Success là ác mộng debug)
        if (errorDetails.Count > 0)
        {
            return Result.Failure(errorDetails);
        }

        // 5. Execution (Side Effects)
        if (brandsToDelete.Count > 0)
        {
            foreach (var brand in brandsToDelete)
            {
                deleteRepository.Delete(brand);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Result.Success();
    }
}