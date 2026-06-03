using Application.ApiContracts.SupplierContracts.Requests;
using Application.ApiContracts.SupplierContracts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.SupplierContracts.Commands.CreateSupplierContract;

public sealed record CreateSupplierContractCommand : IRequest<Result<SupplierContractResponse>>
{
    public int? SupplierId { get; init; }
    public string ContractNumber { get; init; } = string.Empty;
    public string? ContractFilePath { get; init; }
    public DateTime EffectiveDate { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public decimal ContractValue { get; init; }
    public string Status { get; init; } = "Draft";
    public string? Terms { get; init; }
    public string? Note { get; init; }

    public decimal? CreditLimit { get; init; }
    public int? PaymentWindowDays { get; init; }
    public string? BankAccountNumber { get; init; }
    public string? BankName { get; init; }
    public int? MinimumVolumePerMonth { get; init; }
    public decimal? DiscountRate { get; init; }
    public Guid? ParentContractId { get; init; }
    public ICollection<SupplierContractItemDto> ContractItems { get; init; } = [];
}
