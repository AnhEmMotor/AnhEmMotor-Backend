using Application.ApiContracts.FinanceContract.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.FinanceContracts.Mappings;

public class FinanceContractMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<FinanceContract, FinanceContractDetailResponse>()
            .Map(dest => dest.Status, src => src.DisbursementStatus)
            .Map(dest => dest.Customer360, src => FinanceContractCustomer360Catalog.GetCustomer360(src.ContractNumber))
            .Map(dest => dest.FinancialPartner, src => new PartnerResponse { Name = src.BankName })
            .Map(
                dest => dest.CreditPackage,
                src => new CreditPackageResponse
                {
                    PrincipalAmount = src.LoanAmount,
                    TermMonths = src.TermMonths,
                    InterestRateRange = $"{src.InterestRate}%",
                })
            .Map(
                dest => dest.Disbursement,
                src => new DisbursementResponse
                {
                    ExpectedDate = src.DisbursementStatus == "Pending" ? src.SignedDate : null,
                    ActualDate = src.DisbursementStatus == "Disbursed" ? src.SignedDate : null,
                    ExpectedAmount = src.DisbursementStatus == "Pending" ? src.LoanAmount : null,
                    ActualAmount = src.DisbursementStatus == "Disbursed" ? src.LoanAmount : null,
                })
            .Map(dest => dest.Cavet, src => new CavetResponse { State = MapCavetState(src.CavetLocation), });
    }

    private static string MapCavetState(string? location) => location?.ToLowerInvariant() switch
    {
        "bank" => "FinancialCompanyHolds",
        "store" => "StoreHoldsOnBehalf",
        "customer" => "DeliveredToCustomer",
        _ => "FinancialCompanyHolds",
    };
}
