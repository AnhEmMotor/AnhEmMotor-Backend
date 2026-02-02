using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreBrand;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using BrandEntities = Domain.Entities.Brand;

namespace UnitTests;

public class Brand
{
    private readonly Mock<IBrandInsertRepository> _insertRepoMock;
    private readonly Mock<IBrandUpdateRepository> _updateRepoMock;
    private readonly Mock<IBrandDeleteRepository> _deleteRepoMock;
    private readonly Mock<IBrandReadRepository> _readRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Brand()
    {
        _insertRepoMock = new Mock<IBrandInsertRepository>();
        _updateRepoMock = new Mock<IBrandUpdateRepository>();
        _deleteRepoMock = new Mock<IBrandDeleteRepository>();
        _readRepoMock = new Mock<IBrandReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "BRAND_002 - Tạo thương hiệu với tên rỗng")]
    public void BRAND_002_CreateBrand_EmptyName_ShouldFailValidation()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = string.Empty, Description = "Desc" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "BRAND_003 - Tạo thương hiệu với tên quá dài (>100 ký tự)")]
    public void BRAND_003_CreateBrand_NameTooLong_ShouldFailValidation()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = new string('a', 101), Description = "Desc" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "BRAND_004 - Tạo thương hiệu với tên đã tồn tại")]
    public async Task BRAND_004_CreateBrand_DuplicateName_ShouldThrowException()
    {
        // Arrange
        var handler = new CreateBrandCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateBrandCommand { Name = "ExistingBrand", Description = "Desc" };

        // Đây là chỗ bạn sai. Hãy sửa lại mock đúng hàm GetByNameAsync
        var existingBrands = new List<BrandEntities> { new() { Name = "ExistingBrand" } };

        _readRepoMock.Setup(x => x.GetByNameAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingBrands);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull(); // Kiểm tra thêm xem có đúng lỗi duplicate không
    }

    [Fact(DisplayName = "BRAND_011 - Cập nhật thương hiệu với tên đã tồn tại")]
    public async Task BRAND_011_UpdateBrand_DuplicateName_ShouldThrowException()
    {
        var handler = new UpdateBrandCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new UpdateBrandCommand { Id = 1, Name = "ExistingBrand", Description = "Desc" };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new BrandEntities { Id = 1, Name = "OldName" });

        var duplicateBrands = new List<BrandEntities> { new() { Id = 2, Name = "ExistingBrand" } };

        _readRepoMock.Setup(x => x.GetByNameAsync(
                "ExistingBrand",
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(duplicateBrands);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation"); // Kiểm tra thêm cái này cho chắc
    }

    [Fact(DisplayName = "BRAND_019 - Validate Tên thương hiệu chứa ký tự đặc biệt không hợp lệ")]
    public void BRAND_019_CreateBrand_SpecialChars_ShouldFailValidation()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = "Brand@#$", Description = "Desc" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "BRAND_020 - Validate Mô tả quá dài (>500 ký tự)")]
    public void BRAND_020_CreateBrand_DescriptionTooLong_ShouldFailValidation()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = "Brand", Description = new string('a', 501) };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact(DisplayName = "BRAND_023 - Unit: CreateBrandCommandHandler - Success")]
    public async Task BRAND_023_CreateBrand_Success()
    {
        var handler = new CreateBrandCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateBrandCommand { Name = "Honda", Description = "Desc" };

        _insertRepoMock.Setup(x => x.Add(It.IsAny<BrandEntities>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(Enumerable.Empty<BrandEntities>().AsQueryable());

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Name.Should().Be("Honda");
    }

    [Fact(DisplayName = "BRAND_024 - Unit: UpdateBrandCommandHandler - Success")]
    public async Task BRAND_024_UpdateBrand_Success()
    {
        var handler = new UpdateBrandCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateBrandCommand { Id = 1, Name = "Honda Updated", Description = "Desc" };

        _updateRepoMock.Setup(x => x.Update(It.IsAny<BrandEntities>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _readRepoMock.Setup(
            x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new BrandEntities { Id = 1, Name = "Honda" });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(Enumerable.Empty<BrandEntities>().AsQueryable());

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _updateRepoMock.Verify(x => x.Update(It.IsAny<BrandEntities>()), Times.Once);
    }

    [Fact(DisplayName = "BRAND_025 - Unit: DeleteBrandCommandHandler - Success")]
    public async Task BRAND_025_DeleteBrand_Success()
    {
        // Arrange
        var handler = new DeleteBrandCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteBrandCommand { Id = 1 };

        // 1. QUAN TRỌNG: Phải giả lập là tìm thấy Brand trong DB
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new BrandEntities { Id = 1, Name = "To Be Deleted" });

        _deleteRepoMock.Setup(x => x.Delete(It.IsAny<BrandEntities>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        // 2. Sửa lại logic verify kết quả
        result.IsSuccess.Should().BeTrue(); // Success thì phải là True

        _deleteRepoMock.Verify(x => x.Delete(It.IsAny<BrandEntities>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "BRAND_026 - Unit: RestoreBrandCommandHandler - Success")]
    public async Task BRAND_026_RestoreBrand_Success()
    {
        // Arrange
        var handler = new RestoreBrandCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreBrandCommand { Id = 1 };

        // 1. CẤP DỮ LIỆU: Giả lập tìm thấy Brand đã xóa trong thùng rác
        // Lưu ý: Tham số thứ 3 phải match với code thật (DataFetchMode.DeletedOnly) hoặc dùng It.IsAny
        _readRepoMock.Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly)) // <--- Quan trọng
            .ReturnsAsync(new BrandEntities { Id = 1, Name = "Restored Brand", DeletedAt = DateTimeOffset.Now });

        // Setup UnitOfWork
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify hàm Restore được gọi 1 lần
        _updateRepoMock.Verify(x => x.Restore(It.IsAny<BrandEntities>()), Times.Once);

        // Verify SaveChanges được gọi 1 lần
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "BRAND_027 - Unit: GetBrandByIdQueryHandler - Found")]
    public async Task BRAND_027_GetBrandById_Found()
    {
        var handler = new GetBrandByIdQueryHandler(_readRepoMock.Object);
        var query = new GetBrandByIdQuery { Id = 1 };
        var brand = new BrandEntities { Id = 1, Name = "Honda" };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(brand);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value?.Id.Should().Be(1);
    }

    [Fact(DisplayName = "BRAND_028 - Unit: GetBrandByIdQueryHandler - NotFound")]
    public async Task BRAND_028_GetBrandById_NotFound()
    {
        var handler = new GetBrandByIdQueryHandler(_readRepoMock.Object);
        var query = new GetBrandByIdQuery { Id = 999 };

        _readRepoMock.Setup(
            x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((BrandEntities?)null);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "BRAND_029 - Trim tên thương hiệu khi tạo")]
    public async Task BRAND_029_CreateBrand_TrimName_Success()
    {
        var handler = new CreateBrandCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateBrandCommand { Name = "  Honda  ", Description = "Desc" };

        _insertRepoMock.Setup(x => x.Add(It.IsAny<BrandEntities>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(Enumerable.Empty<BrandEntities>().AsQueryable());

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Value.Name.Should().Be("Honda");
    }

    [Fact(DisplayName = "BRAND_030 - Trim tên thương hiệu khi cập nhật")]
    public async Task BRAND_030_UpdateBrand_TrimName_Success()
    {
        var handler = new UpdateBrandCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateBrandCommand { Id = 1, Name = "  Honda Updated  ", Description = "Desc" };

        _updateRepoMock.Setup(x => x.Update(It.IsAny<BrandEntities>()))
            .Callback<BrandEntities>(b => b.Name.Should().Be("Honda Updated"));

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _readRepoMock.Setup(
            x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(new BrandEntities { Id = 1, Name = "Honda" });
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(Enumerable.Empty<BrandEntities>().AsQueryable());

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _updateRepoMock.Verify(x => x.Update(It.IsAny<BrandEntities>()), Times.Once);
    }

    [Fact(DisplayName = "BRAND_040 - Unit: DeleteManyBrandsCommandHandler - Success")]
    public async Task BRAND_040_DeleteManyBrands_Success()
    {
        // 1. Arrange
        var handler = new DeleteManyBrandsCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new DeleteManyBrandsCommand { Ids = [1, 2] };

        // SỬA: Dùng List<Brand> thay vì List<BrandEntities>
        // Để khớp với kiểu trả về của Interface IBrandReadRepository
        var existingBrands = new List<BrandEntities>
    {
        new() { Id = 1, Name = "Brand 1", DeletedAt = null },
        new() { Id = 2, Name = "Brand 2", DeletedAt = null }
    };

        _readRepoMock.Setup(x => x.GetByIdAsync(
                // Logic match ID này ổn, giữ nguyên
                It.Is<List<int>>(ids => ids.Contains(1) && ids.Contains(2)),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingBrands);

        // 2. Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // 3. Assert
        Assert.True(result.IsSuccess); // Dòng này quan trọng, nếu logic sai nó sẽ chặn ngay tại đây

        // SỬA: Verify với kiểu Brand thay vì BrandEntities
        _deleteRepoMock.Verify(x => x.Delete(It.Is<BrandEntities>(b => b.Id == 1)), Times.Once);
        _deleteRepoMock.Verify(x => x.Delete(It.Is<BrandEntities>(b => b.Id == 2)), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "BRAND_041 - Unit: RestoreManyBrandsCommandHandler - Success")]
    public async Task BRAND_041_RestoreManyBrands_Success()
    {
        var handler = new RestoreManyBrandsCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new RestoreManyBrandsCommand { Ids = [1, 2] };

        var mockBrands = new List<BrandEntities>
        {
            new() { Id = 1, Name = "Brand 1", DeletedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Brand 2", DeletedAt = DateTime.UtcNow }
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(
                It.Is<List<int>>(ids => ids.Contains(1) && ids.Contains(2)),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockBrands);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _updateRepoMock.Verify(x => x.Restore(It.Is<List<BrandEntities>>(l => l.Count == 2)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact(DisplayName = "BRAND_042 - Unit: CreateBrand - Description Null")]
    public void BRAND_042_CreateBrand_DescriptionNull_Success()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = "Honda", Description = null };

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact(DisplayName = "BRAND_043 - Unit: UpdateBrand - Description Null")]
    public void BRAND_043_UpdateBrand_DescriptionNull_Success()
    {
        var validator = new UpdateBrandCommandValidator();
        var command = new UpdateBrandCommand { Id = 1, Name = "Honda", Description = null };

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact(DisplayName = "BRAND_047 - Unit: CreateBrand - Name with Special Chars (Valid)")]
    public void BRAND_047_CreateBrand_NameWithSpecialChars_Valid()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = "Honda-Vietnam", Description = "Desc" };

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "BRAND_048 - Unit: CreateBrand - Name with Numbers (Valid)")]
    public void BRAND_048_CreateBrand_NameWithNumbers_Valid()
    {
        var validator = new CreateBrandCommandValidator();
        var command = new CreateBrandCommand { Name = "Brand 123", Description = "Desc" };

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
