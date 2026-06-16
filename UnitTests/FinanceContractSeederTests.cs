using FluentAssertions;
using Application.ApiContracts.FinanceContract.Responses;
using Application.Features.FinanceContracts.Mappings;
using Infrastructure.Seeders;
using Mapster;

namespace UnitTests;

public class FinanceContractSeederTests
{
    [Fact(DisplayName = "FINANCE_CONTRACT_SEED_001 - Seed hop dong tai chinh tra gop co du du lieu hien thi")]
    public void CreateSeedContracts_ShouldContainInstallmentFinanceDemoData()
    {
        var currentDate = new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero);

        var contracts = FinanceContractSeeder.CreateSeedContracts(currentDate);

        contracts.Should().HaveCount(5);
        contracts.Select(x => x.ContractNumber).Should().OnlyHaveUniqueItems();
        contracts.Should().OnlyContain(x => x.LoanAmount > 0);
        contracts.Should().OnlyContain(x => x.TermMonths > 0);
        contracts.Should().Contain(x => x.DisbursementStatus == "Pending");
        contracts.Should().Contain(x => x.DisbursementStatus == "Disbursed");
        contracts.Should().Contain(x => x.DisbursementStatus == "Pending" && x.SignedDate < currentDate.UtcDateTime);
        contracts.Select(x => x.CavetLocation).Should().Contain(["Bank", "Store", "Customer"]);
        contracts.Should().OnlyContain(x => x.CustomerId.HasValue);
    }

    [Fact(DisplayName = "FINANCE_CONTRACT_SEED_002 - Mapping seed co thong tin khach hang 360")]
    public void FinanceContractMapping_ShouldMapDemoCustomer360Data()
    {
        var config = new TypeAdapterConfig();
        new FinanceContractMapsterRegister().Register(config);
        var contract = FinanceContractSeeder
            .CreateSeedContracts(new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero))
            .First(x => x.ContractNumber == "TG-HDSAISON-2026-001");

        var response = contract.Adapt<FinanceContractDetailResponse>(config);

        response.Customer360.Should().NotBeNull();
        response.Customer360!.FullName.Should().Be("Nguyen Minh Quan");
        response.Customer360.Cccd.Should().Be("079202006001");
        response.Customer360.Address.Should().Contain("TP. Ho Chi Minh");
    }
}
