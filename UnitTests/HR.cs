using Domain.Entities.HR;
using FluentAssertions;

namespace UnitTests;

public class HR
{
    [Fact(DisplayName = "HR03 - Khởi tạo chính sách hoa hồng theo phần trăm thành công")]
    public void HR03_Initialize_Percentage_Policy_Success()
    {
        // Arrange
        var policy = new CommissionPolicy
        {
            ProductId = 1,
            Type = "Percentage",
            Value = 5,
            EffectiveDate = DateTimeOffset.UtcNow,
            IsActive = true
        };

        // Assert
        policy.Value.Should().Be(5);
        policy.Type.Should().Be("Percentage");
        policy.Value.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "HR08 - Khởi tạo chính sách hoa hồng mức cố định thành công")]
    public void HR08_Initialize_FixedAmount_Policy_Success()
    {
        // Arrange
        var policy = new CommissionPolicy
        {
            ProductId = 1,
            Type = "FixedAmount",
            Value = 500000,
            EffectiveDate = DateTimeOffset.UtcNow,
            IsActive = true
        };

        // Assert
        policy.Value.Should().Be(500000);
        policy.Type.Should().Be("FixedAmount");
        policy.Value.Should().BeGreaterThan(0);
    }
}
