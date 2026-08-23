using Application.Features.SupplierContracts.Commands.CreateSupplierContract;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace UnitTests;

public class SupplierContractCreation
{
    [Fact]
    public void CreateContract_RejectsInvalidDatesStatusAndValue()
    {
        var validator = new CreateSupplierContractCommandValidator();
        var command = new CreateSupplierContractCommand
        {
            ContractNumber = "HD-NCC-INVALID",
            EffectiveDate = new DateTime(2026, 8, 20),
            ExpirationDate = new DateTime(2026, 8, 19),
            ContractValue = -1,
            Status = "Unknown"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ExpirationDate);
        result.ShouldHaveValidationErrorFor(x => x.ContractValue);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void CreateContract_RejectsDuplicateItemsAndNegativeWholesalePrice()
    {
        var validator = new CreateSupplierContractCommandValidator();
        var command = new CreateSupplierContractCommand
        {
            ContractNumber = "HD-NCC-ITEMS",
            EffectiveDate = new DateTime(2026, 8, 20),
            Status = "Draft",
            ContractItems =
            [
                new() { ProductVariantId = 10, WholesalePrice = 100 },
                new() { ProductVariantId = 10, WholesalePrice = -1 }
            ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ContractItems);
        result.Errors.Should().Contain(error => error.PropertyName.Contains("ContractItems[1].WholesalePrice"));
    }
}
