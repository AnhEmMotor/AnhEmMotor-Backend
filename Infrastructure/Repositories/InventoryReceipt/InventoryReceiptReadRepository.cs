using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;

namespace Infrastructure.Repositories.InventoryReceipt
{
    public class InventoryReceiptReadRepository(ApplicationDBContext context, ISievePaginator paginator) : IInventoryReceiptReadRepository
    {
        public Task<InventoryReceiptStatsResponse> GetStatsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<InventoryReceiptEntity, bool>>? filter = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable(mode);
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return paginator.ApplyAsync<InventoryReceiptEntity, TResponse>(query, sieveModel, mode, cancellationToken);
        }

        internal IQueryable<InventoryReceiptEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = context.InventoryReceipts.IgnoreQueryFilters();
            if (mode == DataFetchMode.ActiveOnly)
            {
                query = query.Where(x => x.DeletedAt == null);
            }
            else if (mode == DataFetchMode.DeletedOnly)
            {
                query = query.Where(x => x.DeletedAt != null);
            }
            return query;
        }

        public Task<IEnumerable<InventoryReceiptEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<InventoryReceiptEntity>>(t => t.Result, cancellationToken);
        }

        public Task<InventoryReceiptEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
        }

        public Task<IEnumerable<InventoryReceiptEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            var query = GetQueryable(mode);
            return query
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken)
                .ContinueWith<IEnumerable<InventoryReceiptEntity>>(t => t.Result, cancellationToken);
        }

        public Task<InventoryReceiptEntity?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            return GetQueryable(mode)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                .ContinueWith(t => t.Result, cancellationToken);
        }

        public Task<List<InventoryReceiptEntity>> GetBySupplierIdAsync(
            int supplierId,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryReceiptEntity>> GetBySupplierIdsAsync(
            IEnumerable<int> supplierIds,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly)
        {
            throw new NotImplementedException();
        }

        public Task<Domain.Entities.InventoryReceiptInfo?> GetInfoByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.InventoryReceiptInfo>> GetInfosByVariantAsync(
            int variantId,
            int? colorId,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
