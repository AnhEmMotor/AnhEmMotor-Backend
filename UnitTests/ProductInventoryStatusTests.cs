using Application.Features.Products.Mappings;
using Domain.Constants;
using Domain.Constants.Input;
using Domain.Constants.Order;
using Domain.Entities;
using FluentAssertions;
using ProductEntity = Domain.Entities.Product;

namespace UnitTests;

public class ProductInventoryStatusTests
{
    [Fact(DisplayName = "PRODUCT_128 - CalculateInventoryStatus - Trường hợp Còn hàng")]
    public void CalculateInventoryStatus_AvailableMoreThanAlert_ReturnsInStock()
    {
        // Arrange
        long availableStock = 10;
        long alertLevel = 5;

        // Act
        var result = ProductMappingConfig.CalculateInventoryStatus(availableStock, alertLevel);

        // Assert
        result.Should().Be(InventoryStatus.InStock);
    }

    [Fact(DisplayName = "PRODUCT_129 - CalculateInventoryStatus - Trường hợp Sắp hết hàng")]
    public void CalculateInventoryStatus_AvailableLessThanOrEqualToAlert_ReturnsLowStock()
    {
        // Arrange
        long availableStock = 3;
        long alertLevel = 5;

        // Act
        var result = ProductMappingConfig.CalculateInventoryStatus(availableStock, alertLevel);

        // Assert
        result.Should().Be(InventoryStatus.LowStock);
    }

    [Fact(DisplayName = "PRODUCT_130 - CalculateInventoryStatus - Trường hợp Hết hàng")]
    public void CalculateInventoryStatus_AvailableZeroOrLess_ReturnsOutOfStock()
    {
        // Arrange
        long availableStock = 0;
        long alertLevel = 5;

        // Act
        var result = ProductMappingConfig.CalculateInventoryStatus(availableStock, alertLevel);

        // Assert
        result.Should().Be(InventoryStatus.OutOfStock);
    }

    [Fact(DisplayName = "PRODUCT_131 - CalculateProductInventoryStatus - Lấy trạng thái tệ nhất")]
    public void CalculateProductInventoryStatus_MixedVariants_ReturnsWorstStatus()
    {
        // Arrange
        long alertLevel = 5;
        var product = new ProductEntity
        {
            ProductVariants =
            [
                new ProductVariant
                {
                    InputInfos = [ new InputInfo { RemainingCount = 10, InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish } } ],
                    OutputInfos = []
                },
                new ProductVariant
                {
                    InputInfos = [ new InputInfo { RemainingCount = 3, InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish } } ],
                    OutputInfos = []
                },
                new ProductVariant
                {
                    InputInfos = [ new InputInfo { RemainingCount = 0, InputReceipt = new Input { StatusId = Domain.Constants.Input.InputStatus.Finish } } ],
                    OutputInfos = []
                }
            ]
        };

        // Act
        var result = ProductMappingConfig.CalculateProductInventoryStatus(product, alertLevel);

        // Assert
        // InStock ("in-stock"), LowStock ("low-stock"), OutOfStock ("out-of-stock")
        // Worst is OutOfStock (Severity 1)
        result.Should().Be(InventoryStatus.OutOfStock);
    }
}
