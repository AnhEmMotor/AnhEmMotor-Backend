using Application.Features.FinanceContracts.Queries.GetFinanceContractsList;
using FluentAssertions;
using Infrastructure.Seeders;
using Sieve.Models;

namespace UnitTests;

public class FinanceContractKeywordFilterTests
{
    [Theory(DisplayName = "FINANCE_CONTRACT_FILTER_001 - Keyword tim thay so hop dong va ten khach hang")]
    [InlineData("nguyen", "TG-HDSAISON-2026-001")]
    [InlineData("Ngoc Anh", "TG-FECREDIT-2026-002")]
    [InlineData("TG-TPBANK", "TG-TPBANK-2026-005")]
    public void Apply_ShouldMatchContractNumberAndCustomerName(
        string keyword,
        string expectedContractNumber)
    {
        var contracts = FinanceContractSeeder
            .CreateSeedContracts(new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero))
            .AsQueryable();
        var sieveModel = new SieveModel { Filters = $"ContractNumber@={keyword}" };

        var result = FinanceContractKeywordFilter.Apply(contracts, sieveModel);

        result.Query.Select(x => x.ContractNumber).Should().ContainSingle().Which
            .Should().Be(expectedContractNumber);
        result.SieveModel.Filters.Should().BeNullOrEmpty();
    }

    [Theory(DisplayName = "FINANCE_CONTRACT_FILTER_004 - Keyword khong tim theo CCCD dia chi va doi tac")]
    [InlineData("052199305678")]
    [InlineData("Tran Phu")]
    [InlineData("FE Credit")]
    public void Apply_ShouldNotMatchCccdAddressOrPartner(string keyword)
    {
        var contracts = FinanceContractSeeder
            .CreateSeedContracts(new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero))
            .AsQueryable();
        var sieveModel = new SieveModel { Filters = $"ContractNumber@={keyword}" };

        var result = FinanceContractKeywordFilter.Apply(contracts, sieveModel);

        result.Query.Should().BeEmpty();
        result.SieveModel.Filters.Should().BeNullOrEmpty();
    }

    [Fact(DisplayName = "FINANCE_CONTRACT_FILTER_002 - Keyword filter giu lai cac filter Sieve khac")]
    public void Apply_ShouldPreserveRemainingSieveFilters()
    {
        var contracts = FinanceContractSeeder
            .CreateSeedContracts(new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero))
            .AsQueryable();
        var sieveModel = new SieveModel
        {
            Filters = "ContractNumber@=Phuc,DisbursementStatus==Pending",
            Page = 2,
            PageSize = 5,
            Sorts = "-createdAt"
        };

        var result = FinanceContractKeywordFilter.Apply(contracts, sieveModel);

        result.Query.Select(x => x.ContractNumber).Should().ContainSingle().Which
            .Should().Be("TG-HOMECREDIT-2026-003");
        result.SieveModel.Filters.Should().Be("DisbursementStatus==Pending");
        result.SieveModel.Page.Should().Be(2);
        result.SieveModel.PageSize.Should().Be(5);
        result.SieveModel.Sorts.Should().Be("-createdAt");
    }

    [Fact(DisplayName = "FINANCE_CONTRACT_FILTER_003 - Keyword alias duoc tach khoi Sieve filter")]
    public void Apply_ShouldSupportKeywordAlias()
    {
        var contracts = FinanceContractSeeder
            .CreateSeedContracts(new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero))
            .AsQueryable();
        var sieveModel = new SieveModel { Filters = "Keyword@=Ngoc Anh" };

        var result = FinanceContractKeywordFilter.Apply(contracts, sieveModel);

        result.Query.Select(x => x.ContractNumber).Should().ContainSingle().Which
            .Should().Be("TG-FECREDIT-2026-002");
        result.SieveModel.Filters.Should().BeNullOrEmpty();
    }
}
