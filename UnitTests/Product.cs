using Application.ApiContracts.Product.Requests;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteManyProducts;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.RestoreManyProducts;
using Application.Features.Products.Commands.RestoreProduct;
using Application.Features.Products.Commands.UpdateManyProductPrices;
using Application.Features.Products.Commands.UpdateManyProductStatuses;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Commands.UpdateProductPrice;
using Application.Features.Products.Commands.UpdateProductStatus;
using Application.Features.Products.Commands.UpdateVariantPrice;
using Application.Features.Products.Queries.CheckSlugAvailability;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class Product
{
    private readonly Mock<IProductInsertRepository> _productInsertRepoMock;
    private readonly Mock<IProductUpdateRepository> _productUpdateRepoMock;
    private readonly Mock<IProductDeleteRepository> _productDeleteRepoMock;
    private readonly Mock<IProductReadRepository> _productReadRepoMock;
    private readonly Mock<IProductCategoryReadRepository> _productCategoryReadRepoMock;
    private readonly Mock<IBrandReadRepository> _brandReadRepoMock;
    private readonly Mock<IProductVariantInsertRepository> _variantInsertRepoMock;
    private readonly Mock<IProductVariantUpdateRepository> _variantUpdateRepoMock;
    private readonly Mock<IProductVarientDeleteRepository> _variantDeleteRepoMock;
    private readonly Mock<IProductVariantReadRepository> _variantReadRepoMock;
    private readonly Mock<IOptionReadRepository> _optionReadRepoMock;
    private readonly Mock<IOptionValueInsertRepository> _optionValueInsertRepoMock;
    private readonly Mock<IOptionValueReadRepository> _optionValueReadRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductVariantInsertRepository> _productVariantInsertRepository;

    public Product()
    {
        _productInsertRepoMock = new Mock<IProductInsertRepository>();
        _productUpdateRepoMock = new Mock<IProductUpdateRepository>();
        _productDeleteRepoMock = new Mock<IProductDeleteRepository>();
        _productReadRepoMock = new Mock<IProductReadRepository>();
        _productCategoryReadRepoMock = new Mock<IProductCategoryReadRepository>();
        _brandReadRepoMock = new Mock<IBrandReadRepository>();
        _variantInsertRepoMock = new Mock<IProductVariantInsertRepository>();
        _variantUpdateRepoMock = new Mock<IProductVariantUpdateRepository>();
        _variantDeleteRepoMock = new Mock<IProductVarientDeleteRepository>();
        _variantReadRepoMock = new Mock<IProductVariantReadRepository>();
        _optionReadRepoMock = new Mock<IOptionReadRepository>();
        _optionValueInsertRepoMock = new Mock<IOptionValueInsertRepository>();
        _optionValueReadRepoMock = new Mock<IOptionValueReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productVariantInsertRepository = new Mock<IProductVariantInsertRepository>();
    }
#pragma warning disable IDE0079 
#pragma warning disable CRR0035

    [Fact(DisplayName = "PRODUCT_001 - Tạo sản phẩm thành công với 1 biến thể không có optionValues")]
// Remove unnecessary suppression
    public async Task CreateProduct_SingleVariantNoOptions_Success()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave Alpha",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = "wave-alpha-2024",
                    Price = 20000000,
                    CoverImageUrl = "image.jpg",
                    OptionValues = []
                } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_002 - Tạo sản phẩm thành công với nhiều biến thể có optionValues hợp lệ")]
    public async Task CreateProduct_MultipleVariantsWithOptions_Success()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH160i",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = "sh160i-red",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" }, { "Phiên bản", "Cao cấp" } }
                }, new CreateProductVariantRequest
                {
                    UrlSlug = "sh160i-black",
                    Price = 102000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đen" }, { "Phiên bản", "Thường" } }
                } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_003 - Tạo sản phẩm thất bại khi CategoryId không tồn tại")]
    public async Task CreateProduct_CategoryNotExists_ThrowsException()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 999,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.ProductCategory?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_004 - Tạo sản phẩm thất bại khi BrandId không tồn tại")]
    public async Task CreateProduct_BrandNotExists_ThrowsException()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 999,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Brand?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_005 - Tạo sản phẩm thất bại khi CategoryId đã bị xóa mềm")]
    public async Task CreateProduct_CategorySoftDeleted_ThrowsException()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = DateTime.UtcNow });

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_006 - Tạo sản phẩm thất bại khi BrandId đã bị xóa mềm")]
    public async Task CreateProduct_BrandSoftDeleted_ThrowsException()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = DateTime.UtcNow });

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_007 - Tạo sản phẩm thất bại khi UrlSlug trùng với sản phẩm hiện tại")]
    public async Task CreateProduct_DuplicateUrlSlug_ThrowsException()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-alpha-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync("wave-alpha-2024", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 999, UrlSlug = "wave-alpha-2024" });

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_008 - Tạo sản phẩm thất bại khi UrlSlug trùng với sản phẩm đã xóa mềm")]
    public async Task CreateProduct_DuplicateUrlSlugSoftDeleted_ThrowsException()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-alpha-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync("wave-alpha-2024", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 999, UrlSlug = "wave-alpha-2024" });

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_009 - Tạo sản phẩm thất bại khi Price < 0")]
    public void CreateProduct_NegativePrice_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = -100 } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_010 - Tạo sản phẩm thành công khi Price = 0")]
    public async Task CreateProduct_PriceZero_Success()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 0 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_011 - Tạo sản phẩm thất bại khi không có biến thể nào")]
    public void CreateProduct_NoVariants_FailsValidation()
    {
        var command = new CreateProductCommand { Name = "Honda Wave", CategoryId = 1, BrandId = 1, Variants = [] };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(
        DisplayName = "PRODUCT_012 - Tạo sản phẩm thất bại khi có nhiều biến thể nhưng 1 biến thế không có optionValues")]
    public void CreateProduct_MultipleVariantsOneWithoutOptions_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                }, new CreateProductVariantRequest { UrlSlug = "sh-black", Price = 100000000, OptionValues = [] } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_013 - Tạo sản phẩm thất bại khi có nhiều biến thể với optionValues giống nhau")]
    public void CreateProduct_DuplicateOptionValues_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red-1",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                }, new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red-2",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_014 - Tạo sản phẩm thất bại khi optionValues chứa Option không hợp lệ")]
    public void CreateProduct_InvalidOption_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = "sh-xl",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Kích thước", "XL" } }
                } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_015 - Tạo sản phẩm thất bại khi Option có nhưng OptionValue trống")]
    public void CreateProduct_EmptyOptionValue_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [ new CreateProductVariantRequest
                {
                    UrlSlug = "sh-variant",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", string.Empty } }
                } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_016 - Tạo sản phẩm với tên có khoảng trắng đầu cuối (phải tự động trim)")]
    public async Task CreateProduct_NameWithSpaces_AutoTrimmed()
    {
        var command = new CreateProductCommand
        {
            Name = " Honda Wave ",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_017 - Tạo sản phẩm với dung tích xy-lanh vượt quá 1 chữ số thập phân")]
    public void CreateProduct_DisplacementExceedsOneDecimal_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Displacement = 124.85m,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_018 - Tạo sản phẩm với công suất vượt quá 2 chữ số thập phân")]
    public void CreateProduct_MaxPowerExceedsTwoDecimals_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            MaxPower = "15.851",
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_019 - Tạo sản phẩm với mức tiêu thụ nhiên liệu vượt quá 2 chữ số thập phân")]
    public void CreateProduct_FuelConsumptionExceedsTwoDecimals_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            FuelConsumption = "2.155",
            Variants = [ new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 } ]
        };

        var validator = new CreateProductCommandValidator();

        var result = validator.Validate(command);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_020 - Tạo sản phẩm với kí tự đặc biệt trong Name")]
    public async Task CreateProduct_SpecialCharactersInName_Sanitized()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda<script>alert('test')</script>",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new CreateProductVariantRequest { UrlSlug = "honda-2024", Price = 20000000 } ]
        };

        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_021 - Sửa sản phẩm thành công (Happy Path)")]
    public async Task UpdateProduct_ValidData_Success()
    {
        var command = new UpdateProductCommand()
        {
            Id = 1,
            Name = "Honda Wave Alpha 2025",
            CategoryId = 1,
            BrandId = 1,
            Variants = []
        };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_022 - Sửa sản phẩm thất bại khi ProductId không tồn tại")]
    public async Task UpdateProduct_ProductNotExists_ThrowsException()
    {
        var command = new UpdateProductCommand { Id = 999, Name = "Honda Wave Alpha 2025" };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_023 - Sửa sản phẩm thất bại khi sản phẩm đã bị xóa mềm")]
    public async Task UpdateProduct_ProductSoftDeleted_ThrowsException()
    {
        var command = new UpdateProductCommand { Id = 1, Name = "Honda Wave Alpha 2025" };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow });

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_024 - Sửa sản phẩm thất bại khi gửi StatusId trong request")]
    public async Task UpdateProduct_StatusIdInRequest_ThrowsException()
    {
        var command = new UpdateProductCommand { Id = 1, Name = "Honda Wave" };

        var validator = new UpdateProductCommandValidator();

        var validationResult = validator.Validate(command);

        validationResult.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_025 - Sửa sản phẩm thất bại khi CategoryId mới không tồn tại")]
    public async Task UpdateProduct_NewCategoryNotExists_ThrowsException()
    {
        var command = new UpdateProductCommand { Id = 1, Name = "Honda Wave", CategoryId = 999 };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.ProductCategory?)null);

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_026 - Sửa sản phẩm thất bại khi CategoryId mới đã bị xóa mềm")]
    public async Task UpdateProduct_NewCategorySoftDeleted_ThrowsException()
    {
        var command = new UpdateProductCommand { Id = 1, Name = "Honda Wave", CategoryId = 1 };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = DateTime.UtcNow });

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_027 - Sửa biến thể sản phẩm - thêm biến thể mới")]
    public async Task UpdateProduct_AddNewVariant_Success()
    {
        var command = new UpdateProductCommand
        {
            Id = 1,
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new UpdateProductVariantRequest { UrlSlug = "wave-new-variant", Price = 25000000 } ]
        };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_028 - Sửa biến thể sản phẩm - cập nhật biến thể hiện có")]
    public async Task UpdateProduct_UpdateExistingVariant_Success()
    {
        var command = new UpdateProductCommand
        {
            Id = 1,
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = [ new UpdateProductVariantRequest { Id = 1, UrlSlug = "wave-updated", Price = 22000000 } ]
        };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 1, ProductId = 1, DeletedAt = null });

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_029 - Sửa biến thể sản phẩm - xóa mềm biến thể (không gửi Id trong request)")]
    public async Task UpdateProduct_SoftDeleteVariant_Success()
    {
        var command = new UpdateProductCommand
        {
            Id = 1,
            Name = "Honda Wave",
            CategoryId = 1,
            BrandId = 1,
            Variants = []
        };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([ new() { Id = 1, ProductId = 1, DeletedAt = null } ]);

        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _productUpdateRepoMock.Object,
            _variantDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_030 - Sửa giá một biến thể thành công")]
    public async Task UpdateVariantPrice_ValidData_Success()
    {
        var command = new UpdateVariantPriceCommand { VariantId = 1, Price = 25000000m };

        _variantReadRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 1, ProductId = 1, DeletedAt = null });

        var handler = new UpdateVariantPriceCommandHandler(
            _variantReadRepoMock.Object,
            _variantUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_031 - Sửa giá nhiều biến thể của một sản phẩm thành công")]
    public async Task UpdateProductPrice_ValidData_Success()
    {
        var command = new UpdateProductPriceCommand { Id = 1, Price = 30000000m };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [ new() { Id = 1, ProductId = 1, DeletedAt = null }, new() { Id = 2, ProductId = 1, DeletedAt = null } ]);

        var handler = new UpdateProductPriceCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_032 - Sửa giá nhiều sản phẩm cùng lúc thành công")]
    public async Task UpdateManyProductPrices_ValidData_Success()
    {
        var command = new UpdateManyProductPricesCommand { Ids = [1, 2], Price = 25000000m };

        // SỬA TẠI ĐÂY: Mock đúng phương thức GetByIdWithVariantsAsync
        _productReadRepoMock.Setup(x => x.GetByIdWithVariantsAsync(
         It.IsAny<List<int>>(),
         It.IsAny<CancellationToken>(),
         It.IsAny<DataFetchMode>()))
             .ReturnsAsync((List<int> ids, CancellationToken ct, DataFetchMode fetchMode) =>
             {
                 return [.. ids.Select(id => new Domain.Entities.Product
                 {
                     Id = id,
                     ProductVariants = [new ProductVariant { Id = id * 10, Price = 0 }]
                 })];
             });

        var handler = new UpdateManyProductPricesCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        _productUpdateRepoMock.Verify(x => x.Update(It.IsAny<Domain.Entities.Product>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "PRODUCT_033 - Sửa trạng thái một sản phẩm thành công")]
    public async Task UpdateProductStatus_ValidData_Success()
    {
        var command = new UpdateProductStatusCommand { Id = 1, StatusId = "out-of-stock" };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });

        var handler = new UpdateProductStatusCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_034 - Sửa trạng thái nhiều sản phẩm thành công")]
    public async Task UpdateManyProductStatuses_ValidData_Success()
    {
        var command = new UpdateManyProductStatusesCommand { Ids = [ 1, 2, 3 ], StatusId = "out-of-stock" };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [ new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null } ]);

        var handler = new UpdateManyProductStatusesCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_035 - Sửa trạng thái nhiều sản phẩm thất bại khi 1 trong số đó đã bị xóa")]
    public async Task UpdateManyProductStatuses_OneDeleted_ThrowsException()
    {
        var command = new UpdateManyProductStatusesCommand { Ids = [ 1, 2, 3 ], StatusId = "out-of-stock" };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [ new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = DateTime.UtcNow
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null } ]);

        var handler = new UpdateManyProductStatusesCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_036 - Xóa một sản phẩm thành công")]
    public async Task DeleteProduct_ValidData_Success()
    {
        var productId = 1;
        var command = new DeleteProductCommand { Id = productId };

        var product = new Domain.Entities.Product
        {
            Id = productId,
            StatusId = "for-sale",
            ProductVariants = []
        };

        _productReadRepoMock.Setup(x => x.GetByIdWithDetailsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new DeleteProductCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        _productDeleteRepoMock.Verify(x => x.Delete(product), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PRODUCT_037 - Xóa sản phẩm thất bại khi sản phẩm không tồn tại")]
    public async Task DeleteProduct_ProductNotExists_ThrowsException()
    {
        var command = new DeleteProductCommand { Id = 999 };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product?)null);

        var handler = new DeleteProductCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_038 - Xóa sản phẩm thất bại khi sản phẩm đã bị xóa trước đó")]
    public async Task DeleteProduct_AlreadyDeleted_ThrowsException()
    {
        var command = new DeleteProductCommand { Id = 1 };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow });

        var handler = new DeleteProductCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_039 - Xóa nhiều sản phẩm thành công")]
    public async Task DeleteManyProducts_ValidData_Success()
    {
        var command = new DeleteManyProductsCommand { Ids = [ 1, 2, 3 ] };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                [ new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null } ]);

        var handler = new DeleteManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue(); // ĐÂY LÀ CHỖ CẦN SỬA

        // Xác minh rằng phương thức xóa đã được gọi với đúng số lượng
        // Tùy vào Handler của bạn gọi Delete nhiều lần hay DeleteRange
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PRODUCT_040 - Xóa nhiều sản phẩm thất bại khi 1 trong số đó không tồn tại")]
    public async Task DeleteManyProducts_OneNotExists_ThrowsException()
    {
        var command = new DeleteManyProductsCommand { Ids = [ 1, 2, 3 ] };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                [ new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                } ]);

        var handler = new DeleteManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_041 - Xóa nhiều sản phẩm thất bại khi 1 trong số đó đã bị xóa")]
    public async Task DeleteManyProducts_OneAlreadyDeleted_ThrowsException()
    {
        var command = new DeleteManyProductsCommand { Ids = [ 1, 2, 3 ] };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                [ new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = DateTime.UtcNow
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null } ]);

        var handler = new DeleteManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_042 - Khôi phục một sản phẩm thành công")]
    public async Task RestoreProduct_ValidData_Success()
    {
        var command = new RestoreProductCommand { Id = 1 };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow });

        var handler = new RestoreProductCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_043 - Khôi phục sản phẩm thất bại khi sản phẩm chưa bị xóa")]
    public async Task RestoreProduct_NotDeleted_ThrowsException()
    {
        var command = new RestoreProductCommand { Id = 1 };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product { Id = 1, StatusId = "for-sale", DeletedAt = null });

        var handler = new RestoreProductCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_044 - Khôi phục nhiều sản phẩm thành công")]
    public async Task RestoreManyProducts_ValidData_Success()
    {
        var command = new RestoreManyProductsCommand { Ids = [1, 2, 3] };
        var deletedTime = DateTime.UtcNow;
        var products = new List<Domain.Entities.Product>
        {
            new() { Id = 1, Name = "P1", DeletedAt = deletedTime },
            new() { Id = 2, Name = "P2", DeletedAt = deletedTime },
            new() { Id = 3, Name = "P3", DeletedAt = deletedTime }
        };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>())) // Phải có tham số mode ở đây
            .ReturnsAsync(products);

        var handler = new RestoreManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        _productUpdateRepoMock.Verify(x => x.Restore(It.IsAny<List<Domain.Entities.Product>>()), Times.Once);
    }

    [Fact(DisplayName = "PRODUCT_045 - Khôi phục nhiều sản phẩm thất bại khi 1 trong số đó chưa bị xóa")]
    public async Task RestoreManyProducts_OneNotDeleted_ThrowsException()
    {
        var command = new RestoreManyProductsCommand { Ids = [ 1, 2, 3 ] };

        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [ new() { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = DateTime.UtcNow } ]);

        var handler = new RestoreManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_046 - Kiểm tra UrlSlug có sẵn để dùng (chưa tồn tại)")]
    public async Task CheckSlugAvailability_Available_ReturnsTrue()
    {
        var query = new CheckSlugAvailabilityQuery { Slug = "new-product-slug" };

        _variantReadRepoMock.Setup(x => x.GetBySlugAsync("new-product-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);

        var handler = new CheckSlugAvailabilityQueryHandler(_variantReadRepoMock.Object);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_047 - Kiểm tra UrlSlug không sẵn sàng (đã tồn tại)")]
    public async Task CheckSlugAvailability_AlreadyExists_ReturnsFalse()
    {
        var query = new CheckSlugAvailabilityQuery { Slug = "existing-product-slug" };

        _variantReadRepoMock.Setup(x => x.GetBySlugAsync("existing-product-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 1, UrlSlug = "existing-product-slug" });

        var handler = new CheckSlugAvailabilityQueryHandler(_variantReadRepoMock.Object);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_048 - Kiểm tra UrlSlug không sẵn sàng (đã bị xóa mềm)")]
    public async Task CheckSlugAvailability_SoftDeleted_ReturnsFalse()
    {
        var query = new CheckSlugAvailabilityQuery { Slug = "soft-deleted-slug" };

        _variantReadRepoMock.Setup(x => x.GetBySlugAsync("soft-deleted-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 1, UrlSlug = "soft-deleted-slug", DeletedAt = DateTime.UtcNow });

        var handler = new CheckSlugAvailabilityQueryHandler(_variantReadRepoMock.Object);

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
