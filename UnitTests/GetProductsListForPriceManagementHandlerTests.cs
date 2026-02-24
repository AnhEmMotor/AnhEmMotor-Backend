using Application.ApiContracts.Product.Responses;
using Application.Features.Products.Queries.GetProductsListForPriceManagement;
using Application.Interfaces.Repositories.Product;
using Domain.Primitives;
using FluentAssertions;
using Moq;
using Sieve.Models;
using ProductEntity = Domain.Entities.Product;
using ProductVariant = Domain.Entities.ProductVariant;
using VariantOptionValue = Domain.Entities.VariantOptionValue;
using OptionValue = Domain.Entities.OptionValue;

namespace UnitTests;

public class GetProductsListForPriceManagementHandlerTests
{
    private readonly Mock<IProductReadRepository> _repositoryMock;
    private readonly GetProductsListForPriceManagementQueryHandler _handler;

    public GetProductsListForPriceManagementHandlerTests()
    {
        _repositoryMock = new Mock<IProductReadRepository>();
        _handler = new GetProductsListForPriceManagementQueryHandler(_repositoryMock.Object);
    }

    [Fact(DisplayName = "PRODUCT_120 - Handler ánh xạ đúng từ Entity sang DTO lite cho price management")]
    public async Task Handle_ValidData_ReturnsMappedDto()
    {
        // Arrange
        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = 1,
                Name = "Test Product",
                ProductVariants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        Id = 101,
                        Price = 50000,
                        VariantOptionValues = new List<VariantOptionValue>
                        {
                            new VariantOptionValue { OptionValue = new OptionValue { Name = "Red" } },
                            new VariantOptionValue { OptionValue = new OptionValue { Name = "XL" } }
                        }
                    },
                    new ProductVariant
                    {
                        Id = 102,
                        Price = 0,
                        VariantOptionValues = new List<VariantOptionValue>()
                    }
                }
            }
        };

        _repositoryMock.Setup(x => x.GetPagedProductsForPriceManagementAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, products.Count));

        var query = new GetProductsListForPriceManagementQuery { SieveModel = new SieveModel { Page = 1, PageSize = 10 } };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
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
}
