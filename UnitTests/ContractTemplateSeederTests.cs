using FluentAssertions;
using Infrastructure.Seeders;

namespace UnitTests;

public class ContractTemplateSeederTests
{
    [Fact(DisplayName = "CONTRACT_TEMPLATE_SEED_001 - Seed du 3 mau hop dong chinh")]
    public void CreateSeedTemplates_ShouldContainAllContractMenuTemplates()
    {
        var templates = ContractTemplateSeeder.CreateSeedTemplates(
            new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero));

        templates.Should().HaveCount(3);
        templates.Select(x => x.Code).Should().BeEquivalentTo(
            "SALES_CONTRACT_DEFAULT",
            "FINANCE_INSTALLMENT_CONTRACT_DEFAULT",
            "SUPPLIER_CONTRACT_DEFAULT");
        templates.Should().OnlyContain(x => x.IsActive);
        templates.Should().OnlyContain(x => !string.IsNullOrWhiteSpace(x.Content));
    }
}
