using Application.ApiContracts.Supplier.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Constants.InventoryReceipt;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using SupplierEntity = Domain.Entities.Supplier;

namespace Infrastructure.Repositories.Supplier;

public class SupplierReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator,
    ISieveProcessor sieveProcessor) : ISupplierReadRepository
{
    internal IQueryable<SupplierEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode);
    }

    internal IQueryable<SupplierWithTotalInventoryReceiptResponse> GetQueryableWithTotalInventoryReceipt(
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode)
            .GroupJoin(
                context.GetQuery<Domain.Entities.InventoryReceiptInfo>(DataFetchMode.ActiveOnly)
                    .Where(ii => ii.InventoryReceiptReceipt != null && ii.InventoryReceiptReceipt.StatusId == InventoryReceiptStatus.Finish),
                supplier => (int?)supplier.Id,
                InventoryReceiptInfo => InventoryReceiptInfo.QuotationProductRow != null && InventoryReceiptInfo.QuotationProductRow.QuotationReceipt != null ? InventoryReceiptInfo.QuotationProductRow.QuotationReceipt.SupplierId : null,
                (supplier, InventoryReceiptInfos) => new { supplier, InventoryReceiptInfos })
            .Select(
                x => new SupplierWithTotalInventoryReceiptResponse
                {
                    Id = x.supplier.Id,
                    Name = x.supplier.Name!,
                    Phone = x.supplier.Phone,
                    Email = x.supplier.Email,
                    Address = x.supplier.Address,
                    StatusId = x.supplier.StatusId!,
                    CreatedAt = x.supplier.CreatedAt,
                    UpdatedAt = x.supplier.UpdatedAt,
                    DeletedAt = x.supplier.DeletedAt,
                    Notes = x.supplier.Notes,
                    TaxIdentificationNumber = x.supplier.TaxIdentificationNumber,
                    PartnerTypeId = x.supplier.PartnerTypeId ?? "supplier",
                    TotalInventoryReceipt =
                        x.InventoryReceiptInfos.Sum(ii => (long)(ii.Count ?? 0) * (long)(ii.QuotationProductRow != null ? (ii.QuotationProductRow.QuotePrice ?? 0) : 0))
                });
    }

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        return paginator.ApplyAsync<SupplierEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public Task<PagedResult<TResponse>> GetPagedWithTotalInventoryReceiptAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryableWithTotalInventoryReceipt(mode);
        return paginator.ApplyAsync<SupplierWithTotalInventoryReceiptResponse, TResponse>(
            query,
            sieveModel,
            mode,
            cancellationToken);
    }

    public Task<SupplierWithTotalInventoryReceiptResponse?> GetByIdWithTotalInventoryReceiptAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return GetQueryableWithTotalInventoryReceipt(mode).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<SupplierEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode).ToListAsync(cancellationToken);
    }

    public Task<SupplierEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<List<SupplierEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<SupplierEntity>(mode).Where(s => ids.Contains(s.Id)).ToListAsync(cancellationToken);
    }

    public Task<bool> IsNameExistsAsync(
        string name,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.Name, name) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsPhoneExistsAsync(
        string phone,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.Phone, phone) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsTaxIdExistsAsync(
        string taxId,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.TaxIdentificationNumber, taxId) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<bool> IsEmailExistsAsync(
        string email,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(DataFetchMode.All)
            .AnyAsync(
                x => string.Compare(x.Email, email) == 0 && (!excludeId.HasValue || x.Id != excludeId),
                cancellationToken);
    }

    public Task<List<SupplierEntity>> GetFilteredListAsync(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        var filteredQuery = sieveProcessor.Apply(sieveModel, query);
        return filteredQuery.ToListAsync(cancellationToken);
    }

    public async Task<SupplierStatisticsResponse> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(DataFetchMode.ActiveOnly);
        var totalSuppliers = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var financialSuppliers = await query.Where(s => s.PartnerTypeId == PartnerType.Financial)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        var creditSuppliers = await query.Where(s => s.PartnerTypeId == PartnerType.Supplier)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        return new SupplierStatisticsResponse
        {
            TotalSuppliers = totalSuppliers,
            FinancialSuppliers = financialSuppliers,
            CreditSuppliers = creditSuppliers.ToString()
        };
    }
}

