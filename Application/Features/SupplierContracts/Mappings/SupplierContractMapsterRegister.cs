using Application.ApiContracts.SupplierContracts.Responses;
using Domain.Entities;
using Mapster;

namespace Application.Features.SupplierContracts.Mappings;

public class SupplierContractMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SupplierContract, SupplierContractResponse>()
            .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : null);
        config.NewConfig<SupplierContract, SupplierContractDetailResponse>()
            .Inherits<SupplierContract, SupplierContractResponse>()
            .Map(dest => dest.SupplierCode, src => src.Supplier != null ? src.Supplier.TaxIdentificationNumber : null)
            .Map(dest => dest.SupplierContactName, src => src.Supplier != null ? src.Supplier.Name : null)
            .Map(dest => dest.SupplierPhone, src => src.Supplier != null ? src.Supplier.Phone : null)
            .Map(dest => dest.SupplierEmail, src => src.Supplier != null ? src.Supplier.Email : null)
            .Map(dest => dest.SupplierAddress, src => src.Supplier != null ? src.Supplier.Address : null)
            .Map(dest => dest.SkuPriceList, src => src.ContractItems)
            .Map(dest => dest.AuditLogs, src => src.AuditLogs);
        config.NewConfig<SupplierContractItem, SupplierContractItemResponse>()
            .Map(dest => dest.SkuCode, src => src.ProductVariant != null ? src.ProductVariant.SKU : null)
            .Map(
                dest => dest.ProductName,
                src => src.ProductVariant != null && src.ProductVariant.Product != null
                    ? src.ProductVariant.Product.Name
                    : null)
            .Map(
                dest => dest.Category,
                src => src.ProductVariant != null &&
                        src.ProductVariant.Product != null &&
                        src.ProductVariant.Product.ProductCategory != null
                    ? src.ProductVariant.Product.ProductCategory.Name
                    : null)
            .Map(dest => dest.Moq, src => 1);
        config.NewConfig<SupplierContractAuditLog, SupplierContractAuditLogResponse>()
            .Map(dest => dest.Action, src => src.Action)
            .Map(dest => dest.Details, src => src.Details)
            .Map(dest => dest.ChangedBy, src => src.ChangedBy)
            .Map(dest => dest.IpAddress, src => src.IpAddress)
            .Map(dest => dest.OldValue, src => src.OldValue)
            .Map(dest => dest.NewValue, src => src.NewValue)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}
