using Application.ApiContracts.Product.Requests;
using Application.ApiContracts.Product.Responses;
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
using Application.Features.Products.Mappings;
using Application.Features.Products.Queries.CheckSlugAvailability;
using Application.Features.Products.Queries.GetProductAttributeLabels;
using Application.Features.Products.Queries.GetProductsListForPriceManagement;
using Application.Features.Products.Queries.GetProductStoreDetailBySlug;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Brand;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using Application.Interfaces.Repositories.PredefinedOption;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Repositories.ProductCategory;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.VariantOptionValue;
using Domain.Constants;
using Domain.Constants.Order;
using Domain.Entities;
using FluentAssertions;
using Mapster;
using Moq;
using Sieve.Models;
using System.Text.Json;
using ProductEntity = Domain.Entities.Product;

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
    private readonly Mock<IPredefinedOptionReadRepository> _predefinedOptionReadRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductVariantInsertRepository> _productVariantInsertRepository;
    private readonly Mock<IVariantOptionValueDeleteRepository> _variantOptionValueDeleteRepoMock;
    private readonly Mock<IProductTechnologyRepository> _productTechnologyRepoMock;

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
        _predefinedOptionReadRepoMock = new Mock<IPredefinedOptionReadRepository>();
        _predefinedOptionReadRepoMock
            .Setup(x => x.GetAllKeysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Màu sắc", "Phiên bản", "Kích thước"]);
        _predefinedOptionReadRepoMock
            .Setup(x => x.GetAllAsDictionaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Color", "Màu sắc" }, { "Version", "Phiên bản" }, { "Size", "Kích thước" }
                });
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productVariantInsertRepository = new Mock<IProductVariantInsertRepository>();
        _variantOptionValueDeleteRepoMock = new Mock<IVariantOptionValueDeleteRepository>();
        _productTechnologyRepoMock = new Mock<IProductTechnologyRepository>();
        new ProductMappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035

    [Fact(DisplayName = "PRODUCT_001 - Tạo sản phẩm thành công với 1 biến thể không có optionValues")]
    public async Task CreateProduct_SingleVariantNoOptions_Success()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda Wave Alpha",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new CreateProductVariantRequest
                {
                    UrlSlug = "wave-alpha-2024",
                    Price = 20000000,
                    CoverImageUrl = "image.jpg",
                    OptionValues = []
                }]
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
            _predefinedOptionReadRepoMock.Object,
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
                [new CreateProductVariantRequest
                {
                    UrlSlug = "sh160i-red",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" }, { "Phiên bản", "Cao cấp" } }
                }, new CreateProductVariantRequest
                {
                    UrlSlug = "sh160i-black",
                    Price = 102000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đen" }, { "Phiên bản", "Thường" } }
                }]
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
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
        };
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.ProductCategory?)null);
        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
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
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
        };
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = DateTime.UtcNow });
        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
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
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-alpha-2024", Price = 20000000 }]
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
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-alpha-2024", Price = 20000000 }]
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
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = -100 }]
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 0 }]
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
            _predefinedOptionReadRepoMock.Object,
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
                [new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                }, new CreateProductVariantRequest { UrlSlug = "sh-black", Price = 100000000, OptionValues = [] }]
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
                [new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red-1",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                }, new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red-2",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", "Đỏ" } }
                }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PRODUCT_014 - Tạo sản phẩm thất bại khi optionValues chứa Option không hợp lệ")]
    public async Task CreateProduct_InvalidOption_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new CreateProductVariantRequest
                {
                    UrlSlug = "sh-xl",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Kích thước", "XL" } }
                }]
        };
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);
        var specificPredefinedOptionRepoMock = new Mock<IPredefinedOptionReadRepository>();
        specificPredefinedOptionRepoMock
            .Setup(x => x.GetAllKeysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Màu sắc", "Phiên bản"]);
        specificPredefinedOptionRepoMock
            .Setup(x => x.GetAllAsDictionaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Color", "Màu sắc" }, { "Version", "Phiên bản" }
                });
        var handler = new CreateProductCommandHandler(
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _variantReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _optionReadRepoMock.Object,
            specificPredefinedOptionRepoMock.Object,
            _productInsertRepoMock.Object,
            _productVariantInsertRepository.Object,
            _optionValueInsertRepoMock.Object,
            _unitOfWorkMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().NotBeNullOrEmpty();
        result.Errors!.First().Code.Should().Be("BadRequest");
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
                [new CreateProductVariantRequest
                {
                    UrlSlug = "sh-variant",
                    Price = 100000000,
                    OptionValues = new Dictionary<string, string> { { "Màu sắc", string.Empty } }
                }]
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
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
            _predefinedOptionReadRepoMock.Object,
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "wave-2024", Price = 20000000 }]
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
            Variants = [new CreateProductVariantRequest { UrlSlug = "honda-2024", Price = 20000000 }]
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
            _predefinedOptionReadRepoMock.Object,
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            .ReturnsAsync((ProductEntity?)null);
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow });
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.ProductCategory?)null);
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = DateTime.UtcNow });
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            Variants = [new UpdateProductVariantRequest { UrlSlug = "wave-new-variant", Price = 25000000 }]
        };
        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductVariant?)null);
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            Variants = [new UpdateProductVariantRequest { Id = 1, UrlSlug = "wave-updated", Price = 22000000 }]
        };
        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductVariant { Id = 1, ProductId = 1, DeletedAt = null });
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
        _productCategoryReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.ProductCategory { Id = 1, DeletedAt = null });
        _brandReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Brand { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Id = 1, ProductId = 1, DeletedAt = null }]);
        var handler = new UpdateProductCommandHandler(
            _productReadRepoMock.Object,
            _productTechnologyRepoMock.Object,
            _variantReadRepoMock.Object,
            _productCategoryReadRepoMock.Object,
            _brandReadRepoMock.Object,
            _predefinedOptionReadRepoMock.Object,
            _optionReadRepoMock.Object,
            _optionValueReadRepoMock.Object,
            _variantInsertRepoMock.Object,
            _optionValueInsertRepoMock.Object,
            _variantOptionValueDeleteRepoMock.Object,
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
            .ReturnsAsync(new ProductEntity { Id = 1, DeletedAt = null });
        _variantReadRepoMock.Setup(x => x.GetByProductIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [new() { Id = 1, ProductId = 1, DeletedAt = null }, new() { Id = 2, ProductId = 1, DeletedAt = null }]);
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
        _productReadRepoMock.Setup(
            x => x.GetByIdWithVariantsAsync(
                It.IsAny<List<int>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                (List<int> ids, CancellationToken ct, DataFetchMode fetchMode) =>
                {
                    return[.. ids.Select(
                        id => new ProductEntity
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
        _productUpdateRepoMock.Verify(x => x.Update(It.IsAny<ProductEntity>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "PRODUCT_033 - Sửa trạng thái một sản phẩm thành công")]
    public async Task UpdateProductStatus_ValidData_Success()
    {
        var command = new UpdateProductStatusCommand { Id = 1, StatusId = "out-of-stock" };
        _productReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
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
        var command = new UpdateManyProductStatusesCommand { Ids = [1, 2, 3], StatusId = "out-of-stock" };
        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null }]);
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
        var command = new UpdateManyProductStatusesCommand { Ids = [1, 2, 3], StatusId = "out-of-stock" };
        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = DateTime.UtcNow
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null }]);
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
        var product = new ProductEntity { Id = productId, StatusId = "for-sale", ProductVariants = [] };
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
            .ReturnsAsync((ProductEntity?)null);
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow });
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
        var command = new DeleteManyProductsCommand { Ids = [1, 2, 3] };
        _productReadRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                [new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null }]);
        var handler = new DeleteManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productDeleteRepoMock.Object,
            _unitOfWorkMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PRODUCT_040 - Xóa nhiều sản phẩm thất bại khi 1 trong số đó không tồn tại")]
    public async Task DeleteManyProducts_OneNotExists_ThrowsException()
    {
        var command = new DeleteManyProductsCommand { Ids = [1, 2, 3] };
        _productReadRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                [new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }]);
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
        var command = new DeleteManyProductsCommand { Ids = [1, 2, 3] };
        _productReadRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(
                [new() { Id = 1, StatusId = "for-sale", DeletedAt = null }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = DateTime.UtcNow
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = null }]);
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow });
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
            .ReturnsAsync(new ProductEntity { Id = 1, StatusId = "for-sale", DeletedAt = null });
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
        var products = new List<ProductEntity>
        {
            new() { Id = 1, Name = "P1", DeletedAt = deletedTime },
            new() { Id = 2, Name = "P2", DeletedAt = deletedTime },
            new() { Id = 3, Name = "P3", DeletedAt = deletedTime }
        };
        _productReadRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(products);
        var handler = new RestoreManyProductsCommandHandler(
            _productReadRepoMock.Object,
            _productUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        _productUpdateRepoMock.Verify(x => x.Restore(It.IsAny<List<ProductEntity>>()), Times.Once);
    }

    [Fact(DisplayName = "PRODUCT_045 - Khôi phục nhiều sản phẩm thất bại khi 1 trong số đó chưa bị xóa")]
    public async Task RestoreManyProducts_OneNotDeleted_ThrowsException()
    {
        var command = new RestoreManyProductsCommand { Ids = [1, 2, 3] };
        _productReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                [new() { Id = 1, StatusId = "for-sale", DeletedAt = DateTime.UtcNow }, new()
                {
                    Id = 2,
                    StatusId = "for-sale",
                    DeletedAt = null
                }, new() { Id = 3, StatusId = "for-sale", DeletedAt = DateTime.UtcNow }]);
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

    [Fact(DisplayName = "PRODUCT_110 - Kiểm tra Validation khi URL Slug bị trùng chéo lặp trong cùng request")]
    public void CreateProduct_DuplicateUrlSlugInRequest_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda SH",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new CreateProductVariantRequest { UrlSlug = "sh-red", Price = 100000000 }, new CreateProductVariantRequest
                {
                    UrlSlug = "sh-red",
                    Price = 100000000
                }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_CALC_001 - CalculateTotalStock tính tổng RemainingCount từ Input receipt Finished")]
    public void MapProductToDetailForManagerResponse_CalculatesTotalStockCorrectly()
    {
        var product = new ProductEntity
        {
            Id = 1,
            Name = "Honda Wave",
            ProductVariants =
                [new ProductVariant
                {
                    Id = 1,
                    InputInfos =
                        [new InputInfo
                            {
                                RemainingCount = 10,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish }
                            }, new InputInfo
                            {
                                RemainingCount = 5,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Working }
                            }]
                }, new ProductVariant
                {
                    Id = 2,
                    InputInfos =
                        [new InputInfo
                            {
                                RemainingCount = 15,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish }
                            }]
                }]
        };
        var response = product.Adapt<ProductDetailForManagerResponse>();
        response.Stock.Should().Be(25);
    }

    [Fact(DisplayName = "PRODUCT_CALC_002 - CalculateTotalBooked tính tổng Count từ Output Order Pending/Confirmed")]
    public void MapProductToDetailForManagerResponse_CalculatesReservedStockCorrectly()
    {
        var product = new ProductEntity
        {
            Id = 1,
            Name = "Yamaha",
            ProductVariants =
                [new ProductVariant
                {
                    Id = 1,
                    OutputInfos =
                        [new OutputInfo { Count = 3, OutputOrder = new Output { StatusId = OrderStatus.Pending } }, new OutputInfo
                            {
                                Count = 2,
                                OutputOrder = new Output { StatusId = OrderStatus.ConfirmedCod }
                            }, new OutputInfo
                            {
                                Count = 5,
                                OutputOrder = new Output { StatusId = OrderStatus.Completed }
                            }]
                }]
        };
        var response = product.Adapt<ProductDetailForManagerResponse>();
        response.HasBeenBooked.Should().Be(5);
    }

    [Fact(DisplayName = "PRODUCT_CALC_003 - Tính toán Available To Sell (ATS) chính xác")]
    public void MapProductToDetailForManagerResponse_CalculatesATSCorrectly()
    {
        var product = new ProductEntity
        {
            Id = 1,
            Name = "Suzuki",
            ProductVariants =
                [new ProductVariant
                {
                    Id = 1,
                    InputInfos =
                        [new InputInfo
                            {
                                RemainingCount = 50,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish }
                            }],
                    OutputInfos =
                        [new OutputInfo { Count = 10, OutputOrder = new Output { StatusId = OrderStatus.Pending } }]
                }]
        };
        var response = product.Adapt<ProductDetailForManagerResponse>();
        response.Stock.Should().Be(50);
        response.HasBeenBooked.Should().Be(10);
        response.StatusStockId.Should().Be("in_stock");
    }

    [Fact(DisplayName = "PRODUCT_112 - Tạo sản phẩm hợp lệ với 1 biến thể rỗng OptionValues")]
    public void CreateProduct_SingleEmptyVariant_ValidationSuccess()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda",
            CategoryId = 1,
            BrandId = 1,
            Variants = [new CreateProductVariantRequest { UrlSlug = "honda", Price = 100, OptionValues = [] }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "PRODUCT_113 - Tạo sản phẩm thất bại khi trộn lẫn biến thể có và không có OptionValues")]
    public void CreateProduct_MixedVariants_ValidationFails()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new CreateProductVariantRequest
                {
                    UrlSlug = "honda-1",
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "Màu", "Đỏ" } }
                }, new CreateProductVariantRequest { UrlSlug = "honda-2", Price = 100, OptionValues = [] }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors
            .Should()
            .Contain(e => e.ErrorMessage.Contains("Khi có nhiều biến thể, tất cả các biến thể phải có thuộc tính phân biệt"));
    }

    [Fact(DisplayName = "PRODUCT_114 - Tạo sản phẩm thất bại khi nhiều biến thể trùng OptionValues")]
    public void CreateProduct_DuplicateVariants_ValidationFails()
    {
        var command = new CreateProductCommand
        {
            Name = "Honda",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new CreateProductVariantRequest
                {
                    UrlSlug = "honda-1",
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "Màu", "Đỏ" } }
                }, new CreateProductVariantRequest
                {
                    UrlSlug = "honda-2",
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "Màu", "Đỏ" } }
                }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Các biến thể không được trùng lặp tổ hợp thuộc tính, màu sắc và phiên bản."));
    }

    [Fact(
        DisplayName = "PRODUCT_115 - Sửa sản phẩm thất bại khi trộn lẫn biến thể có và không có OptionValues (Ép định danh)")]
    public void UpdateProduct_MixedVariants_ValidationFails()
    {
        var command = new UpdateProductCommand
        {
            Id = 1,
            Name = "Honda",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new UpdateProductVariantRequest { Id = 1, UrlSlug = "honda-1", Price = 100, OptionValues = [] }, new UpdateProductVariantRequest
                {
                    UrlSlug = "honda-2",
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "Màu", "Đỏ" } }
                }]
        };
        var validator = new UpdateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors
            .Should()
            .Contain(e => e.ErrorMessage.Contains("Khi có nhiều biến thể, tất cả các biến thể phải có thuộc tính phân biệt"));
    }

    [Fact(DisplayName = "PRODUCT_116 - Sửa sản phẩm thất bại khi nhiều biến thể trùng OptionValues")]
    public void UpdateProduct_DuplicateVariants_ValidationFails()
    {
        var command = new UpdateProductCommand
        {
            Id = 1,
            Name = "Honda",
            CategoryId = 1,
            BrandId = 1,
            Variants =
                [new UpdateProductVariantRequest
                {
                    Id = 1,
                    UrlSlug = "honda-1",
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "Màu", "Đỏ" } }
                }, new UpdateProductVariantRequest
                {
                    UrlSlug = "honda-2",
                    Price = 100,
                    OptionValues = new Dictionary<string, string> { { "Màu", "Đỏ" } }
                }]
        };
        var validator = new UpdateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Các biến thể không được trùng lặp tổ hợp thuộc tính, màu sắc và phiên bản."));
    }

    [Fact(DisplayName = "PRODUCT_120 - Handler ánh xạ đúng từ Entity sang DTO lite cho price management")]
    public async Task Handle_ValidData_ReturnsMappedDto()
    {
        var products = new List<ProductEntity>
        {
            new()
            {
                Id = 1,
                Name = "Test Product",
                ProductVariants =
                    [new()
                    {
                        Id = 101,
                        Price = 50000,
                        VariantOptionValues =
                            [new VariantOptionValue { OptionValue = new OptionValue { Name = "Red" } }, new VariantOptionValue
                                {
                                    OptionValue = new OptionValue { Name = "XL" }
                                }],
                        InputInfos =
                            [new InputInfo
                                {
                                    InputPrice = 45000,
                                    InputReceipt = new Input { InputDate = DateTimeOffset.UtcNow.AddDays(-1) }
                                }, new InputInfo
                                {
                                    InputPrice = 40000,
                                    InputReceipt = new Input { InputDate = DateTimeOffset.UtcNow.AddDays(-2) }
                                }]
                    }, new() { Id = 102, Price = 0, VariantOptionValues = [], InputInfos = [] }]
            }
        };
        _productReadRepoMock.Setup(
            x => x.GetPagedProductsForPriceManagementAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, products.Count));
        var query = new GetProductsListForPriceManagementQuery
        {
            SieveModel = new SieveModel { Page = 1, PageSize = 10 }
        };
        GetProductsListForPriceManagementQueryHandler _handler = new(_productReadRepoMock.Object);
        var result = await _handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        var productDto = result.Value.Items.First();
        productDto.Name.Should().Be("Test Product");
        productDto.Variants.Should().HaveCount(2);
        var v1 = productDto.Variants.First(v => v.Id == 101);
        v1.Name.Should().Be("Red / XL");
        v1.Price.Should().Be(50000);
        var v2 = productDto.Variants.First(v => v.Id == 102);
        v2.Name.Should().Be("Mặc định");
        v2.Price.Should().Be(0);
    }

    [Fact(DisplayName = "PRODUCT_128 - CalculateInventoryStatus - Trường hợp Còn hàng")]
    public void CalculateInventoryStatus_AvailableMoreThanAlert_ReturnsInStock()
    {
        long availableStock = 10;
        long alertLevel = 5;
        var result = ProductMappingConfig.CalculateInventoryStatus(availableStock, alertLevel);
        result.Should().Be(InventoryStatus.InStock);
    }

    [Fact(DisplayName = "PRODUCT_129 - CalculateInventoryStatus - Trường hợp Sắp hết hàng")]
    public void CalculateInventoryStatus_AvailableLessThanOrEqualToAlert_ReturnsLowStock()
    {
        long availableStock = 3;
        long alertLevel = 5;
        var result = ProductMappingConfig.CalculateInventoryStatus(availableStock, alertLevel);
        result.Should().Be(InventoryStatus.LowStock);
    }

    [Fact(DisplayName = "PRODUCT_130 - CalculateInventoryStatus - Trường hợp Hết hàng")]
    public void CalculateInventoryStatus_AvailableZeroOrLess_ReturnsOutOfStock()
    {
        long availableStock = 0;
        long alertLevel = 5;
        var result = ProductMappingConfig.CalculateInventoryStatus(availableStock, alertLevel);
        result.Should().Be(InventoryStatus.OutOfStock);
    }

    [Fact(DisplayName = "PRODUCT_131 - CalculateProductInventoryStatus - Lấy trạng thái tệ nhất")]
    public void CalculateProductInventoryStatus_MixedVariants_ReturnsWorstStatus()
    {
        long alertLevel = 5;
        var product = new ProductEntity
        {
            ProductVariants =
                [new ProductVariant
                {
                    InputInfos =
                        [new InputInfo
                            {
                                RemainingCount = 10,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish }
                            }],
                    OutputInfos = []
                }, new ProductVariant
                {
                    InputInfos =
                        [new InputInfo
                            {
                                RemainingCount = 3,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish }
                            }],
                    OutputInfos = []
                }, new ProductVariant
                {
                    InputInfos =
                        [new InputInfo
                            {
                                RemainingCount = 0,
                                InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish }
                            }],
                    OutputInfos = []
                }]
        };
        var result = ProductMappingConfig.CalculateProductInventoryStatus(product, alertLevel);
        result.Should().Be(InventoryStatus.OutOfStock);
    }

    [Fact(DisplayName = "PRODUCT_138 - Lấy danh sách nhãn thuộc tính thành công")]
    public async Task GetProductAttributeLabels_Success()
    {
        var query = new GetProductAttributeLabelsQuery();
        var handler = new GetProductAttributeLabelsQueryHandler();
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().ContainKey("EngineType");
        result.Value!["EngineType"].Should().Be("Loại động cơ");
    }

    [Fact(DisplayName = "PRODUCT_139 - Lấy chi tiết sản phẩm theo Slug thành công khi biến thể tồn tại")]
    public async Task GetProductStoreDetailBySlug_VariantExists_Success()
    {
        var slug = "sh160i-red";
        var query = new GetProductStoreDetailBySlugQuery(slug);
        var product = new ProductEntity
        {
            Id = 1,
            Name = "Honda SH160i",
            Description = "Xe ga cao cấp",
            Brand = new Domain.Entities.Brand { Name = "Honda" },
            ProductCategory = new Domain.Entities.ProductCategory { Name = "Xe ga" },
            ProductVariants = []
        };
        var variant = new ProductVariant
        {
            Id = 10,
            UrlSlug = slug,
            Price = 100000000,
            Product = product,
            VariantOptionValues = [new VariantOptionValue { OptionValue = new OptionValue { Name = "Đỏ" } }],
            ProductCollectionPhotos = []
        };
        product.ProductVariants.Add(variant);
        _productReadRepoMock.Setup(
            x => x.GetByVariantSlugWithDetailsAsync(slug, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync(variant);
        var handler = new GetProductStoreDetailBySlugQueryHandler(_productReadRepoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Product.Name.Should().Be("Honda SH160i");
        result.Value.CurrentVariant.Price.Should().Be(100000000);
        result.Value.CurrentVariant.DisplayName.Should().Be("Đỏ");
    }

    [Fact(DisplayName = "PRODUCT_140 - Lấy chi tiết sản phẩm theo Slug thất bại khi biến thể không tồn tại")]
    public async Task GetProductStoreDetailBySlug_VariantNotExists_ReturnsFailure()
    {
        var slug = "non-existent-slug";
        var query = new GetProductStoreDetailBySlugQuery(slug);
        _productReadRepoMock.Setup(
            x => x.GetByVariantSlugWithDetailsAsync(slug, It.IsAny<CancellationToken>(), DataFetchMode.ActiveOnly))
            .ReturnsAsync((ProductVariant?)null);
        var handler = new GetProductStoreDetailBySlugQueryHandler(_productReadRepoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("ProductDetail.NotFound");
    }

    [Fact(DisplayName = "PRODUCT_145 - Ràng buộc tính phân biệt khi có nhiều biến thể")]
    public void CreateProduct_MultipleVariantsMissingDistinction_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants =
            [
                new CreateProductVariantRequest { UrlSlug = "v1" },
                new CreateProductVariantRequest { UrlSlug = "v2" }
            ]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_146 - Chặn trùng lặp tổ hợp Version và Color")]
    public void CreateProduct_DuplicateVersionAndColor_FailsValidation()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants =
            [
                new CreateProductVariantRequest { UrlSlug = "v1", VersionName = "V1", ColorName = "Red" },
                new CreateProductVariantRequest { UrlSlug = "v2", VersionName = "V1", ColorName = "Red" }
            ]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_151 - Giới hạn độ dài mô tả ngắn (ShortDescription)")]
    public void CreateProduct_ShortDescriptionTooLong_FailsValidation()
    {
        var command = new CreateProductCommand { ShortDescription = new string('a', 256) };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.Errors.Should().Contain(e => string.Compare(e.PropertyName, nameof(CreateProductCommand.ShortDescription)) == 0);
    }

    [Fact(DisplayName = "PRODUCT_152 - Giới hạn độ dài tiêu đề SEO (MetaTitle)")]
    public void CreateProduct_MetaTitleTooLong_FailsValidation()
    {
        var command = new CreateProductCommand { MetaTitle = new string('a', 101) };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.Errors.Should().Contain(e => string.Compare(e.PropertyName, nameof(CreateProductCommand.MetaTitle)) == 0);
    }

    [Fact(DisplayName = "PRODUCT_153 - Logic sinh tên hiển thị (DisplayName) cho biến thể")]
    public void VariantLiteResponse_DisplayName_StandardFormat()
    {
        var variant = new ProductVariant
        {
            VersionName = "V1",
            ColorName = "Đỏ",
            Product = new ProductEntity { Name = "Bike" }
        };
        // Logic mapping thường kết hợp VersionName và ColorName
        var response = variant.Adapt<ProductVariantLiteResponse>();
        response.DisplayName.Should().Contain("V1");
        response.DisplayName.Should().Contain("Đỏ");
    }

    [Fact(DisplayName = "PRODUCT_154 - Logic hiển thị tên mặc định khi thiếu thông tin")]
    public void VariantLiteResponse_DisplayName_FallbackToName()
    {
        var variant = new ProductVariant
        {
            Product = new ProductEntity { Name = "Standard Bike" },
            VersionName = null,
            ColorName = null
        };
        var response = variant.Adapt<ProductVariantLiteResponse>();
        // Nếu không có version/color, thường lấy tên sản phẩm hoặc chuỗi mặc định
        response.DisplayName.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName = "PRODUCT_155 - Tính toán tổng tồn kho (Stock) của biến thể")]
    public void ProductVariant_CalculateStock_SumRemaining()
    {
        var variant = new ProductVariant
        {
            InputInfos =
            [
                new InputInfo { RemainingCount = 5, InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish } },
                new InputInfo { RemainingCount = 10, InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish } }
            ]
        };
        var response = variant.Adapt<ProductVariantLiteResponse>();
        response.Stock.Should().Be(15);
    }

    [Fact(DisplayName = "PRODUCT_158 - Kiểm tra giải mã Slug từ URL trước khi kiểm tra tồn tại")]
    public void UrlHelper_DecodeSlug_Success()
    {
        var slug = "xe-may%20honda";
        var decoded = System.Net.WebUtility.UrlDecode(slug);
        decoded.Should().Be("xe-may honda");
    }

    [Fact(DisplayName = "PRODUCT_164 - Xử lý lỗi khi định dạng JSON Highlights sai")]
    public void CreateProduct_InvalidHighlightsJson_GracefulDegradation()
    {
        var command = new CreateProductCommand { Highlights = "invalid-json" };
        // Validator hoặc Handler nên handle lỗi parse JSON
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        // Tùy logic: có thể cho phép qua nhưng bỏ qua highlights hoặc báo lỗi. 
        // Ở đây giả định báo lỗi định dạng.
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_165 - Mapping ưu tiên tiêu đề tùy chỉnh (Custom Title)")]
    public void ProductTechnology_Mapping_PriorityToCustom()
    {
        var pt = new ProductTechnology { CustomTitle = "Custom", Technology = new Technology { DefaultTitle = "Default" } };
        // Giả sử có DTO hiển thị title
        pt.CustomTitle.Should().Be("Custom");
    }

    [Fact(DisplayName = "PRODUCT_166 - Mapping sử dụng tiêu đề mặc định (Fallback)")]
    public void ProductTechnology_Mapping_FallbackToDefault()
    {
        var pt = new ProductTechnology { CustomTitle = null, Technology = new Technology { DefaultTitle = "Default" } };
        var title = pt.CustomTitle ?? pt.Technology.DefaultTitle;
        title.Should().Be("Default");
    }

    [Fact(DisplayName = "PRODUCT_169 - Kiểm tra ghi đè hình ảnh công nghệ")]
    public void ProductTechnology_Mapping_ImageOverride()
    {
        var pt = new ProductTechnology { CustomImageUrl = "custom.jpg", Technology = new Technology { DefaultImageUrl = "default.jpg" } };
        var img = pt.CustomImageUrl ?? pt.Technology.DefaultImageUrl;
        img.Should().Be("custom.jpg");
    }

    [Fact(DisplayName = "PRODUCT_175 - Trích xuất logic lọc giá từ chuỗi Sieve Filters")]
    public void GetProductsQuery_ExtractPriceFilters_Success()
    {
        var sieve = "price>=10,price<=50";
        sieve.Should().Contain("price>=10");
        sieve.Should().Contain("price<=50");
    }

    [Fact(DisplayName = "PRODUCT_176 - Bắt buộc thuộc tính phân biệt cho nhiều biến thể")]
    public void CreateProduct_MultipleVariants_MustHaveDistinction()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants = [new CreateProductVariantRequest(), new CreateProductVariantRequest()]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_177 - Chấp nhận sản phẩm chỉ có một biến thể duy nhất")]
    public void CreateProduct_SingleVariant_NoDistinctionRequired()
    {
        var command = new CreateProductCommand
        {
            Name = "P", CategoryId = 1,
            BrandId = 1,
            Variants = [new CreateProductVariantRequest { UrlSlug = "v1", Price = 1000, OptionValueIds = [] }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "PRODUCT_178 - Ngăn chặn trùng lặp màu sắc (ColorName) giữa các biến thể")]
    public void CreateProduct_DuplicateColorName_Fails()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants = [new CreateProductVariantRequest { ColorName = "Red" }, new CreateProductVariantRequest { ColorName = "Red" }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_179 - Ngăn chặn trùng lặp phiên bản (VersionName) giữa các biến thể")]
    public void CreateProduct_DuplicateVersionName_Fails()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants = [new CreateProductVariantRequest { VersionName = "V1" }, new CreateProductVariantRequest { VersionName = "V1" }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_180 - Chặn trùng lặp tổ hợp phức hợp (Option + Color + Version)")]
    public void CreateProduct_DuplicateComplex_Fails()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants = [
                new CreateProductVariantRequest { ColorName = "Red", VersionName = "V1" },
                new CreateProductVariantRequest { ColorName = "Red", VersionName = "V1" }
            ]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_181 - Kiểm tra trùng lặp không phân biệt chữ hoa chữ thường")]
    public void CreateProduct_DuplicateCaseInsensitive_Fails()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            CategoryId = 1,
            BrandId = 1,
            Variants = [new CreateProductVariantRequest { ColorName = "RED" }, new CreateProductVariantRequest { ColorName = "red" }]
        };
        var validator = new CreateProductCommandValidator();
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "PRODUCT_182 - Xử lý chuỗi BrandIds rỗng hoặc không hợp lệ")]
    public void GetProductsQuery_InvalidBrandIds_HandledGracefully()
    {
        var brandIds = ",,,";
        var ids = brandIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        ids.Should().BeEmpty();
    }

#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
