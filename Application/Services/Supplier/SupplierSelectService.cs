using Application.ApiContracts.Supplier;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services.Supplier;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Application.Services.Supplier
{
    public class SupplierSelectService(ISupplierSelectRepository supplierSelectRepository, ISieveProcessor sieveProcessor) : ISupplierSelectService
    {
        public async Task<(SupplierResponse? Data, ErrorResponse? Error)> GetSupplierByIdAsync(int id, CancellationToken cancellationToken)
        {
            var supplier = await supplierSelectRepository.GetSupplierByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (supplier == null)
            {
                return (
                    null,
                    new ErrorResponse
                    {
                        Errors =
                        [
                            new ErrorDetail { Message = $"Supplier with Id {id} not found." }
                        ]
                    }
                );
            }
            return (
                new SupplierResponse
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    StatusId = supplier.StatusId,
                    Notes = supplier.Notes,
                    Address = supplier.Address
                },
                null
            );
        }

        public async Task<PagedResult<SupplierResponse>> GetSuppliersAsync(SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var query = supplierSelectRepository.GetSuppliers();
            ApplyDefaultsToSieveModel(sieveModel);
            if (sieveModel.Page == null || sieveModel.PageSize == null)
            {
                int pageNumber = sieveModel.Page ?? 1;
                int pageSize = sieveModel.PageSize ?? 1;
                return new PagedResult<SupplierResponse>(
                    [],
                    0,
                    pageNumber,
                    pageSize
                );
            }
            var suppliersQuery = sieveProcessor.Apply(sieveModel, query);
            var suppliers = await suppliersQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);
            var supplierResponses = suppliers.Select(supplier => new SupplierResponse
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                Email = supplier.Email,
                StatusId = supplier.StatusId,
                Notes = supplier.Notes,
                Address = supplier.Address
            }).ToList();
            return new PagedResult<SupplierResponse>(
                supplierResponses,
                totalCount,
                sieveModel.Page.Value,
                sieveModel.PageSize.Value
            );
        }

        public async Task<PagedResult<SupplierResponse>> GetDeletedSuppliersAsync(SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var query = supplierSelectRepository.GetDeletedSuppliers();
            ApplyDefaultsToSieveModel(sieveModel);
            if (sieveModel.Page == null || sieveModel.PageSize == null)
            {
                int pageNumber = sieveModel.Page ?? 1;
                int pageSize = sieveModel.PageSize ?? 1;
                return new PagedResult<SupplierResponse>(
                    [],
                    0,
                    pageNumber,
                    pageSize
                );
            }
            var suppliersQuery = sieveProcessor.Apply(sieveModel, query);
            var suppliers = await suppliersQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var totalCount = await sieveProcessor.Apply(sieveModel, query, applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);
            var supplierResponses = suppliers.Select(supplier => new SupplierResponse
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                Email = supplier.Email,
                StatusId = supplier.StatusId,
                Notes = supplier.Notes,
                Address = supplier.Address
            }).ToList();
            return new PagedResult<SupplierResponse>(
                supplierResponses,
                totalCount,
                sieveModel.Page.Value,
                sieveModel.PageSize.Value
            );
        }

        private static void ApplyDefaultsToSieveModel(SieveModel sieveModel)
        {
            sieveModel.Page ??= 1;
            sieveModel.PageSize ??= 10;

            if (string.IsNullOrWhiteSpace(sieveModel.Sorts))
            {
                sieveModel.Sorts = $"-id";
            }
            else if (!sieveModel.Sorts.Contains(AuditingProperties.CreatedAt, StringComparison.OrdinalIgnoreCase))
            {
                sieveModel.Sorts = $"{sieveModel.Sorts},-id";
            }
        }
    }
}
