using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using Sieve.Models;

namespace Application.Interfaces.Repositories.SupplierContract;

public interface ISupplierContractReadRepository
{
	public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
		SieveModel sieveModel,
		DataFetchMode mode = DataFetchMode.ActiveOnly,
		CancellationToken cancellationToken = default);

	public Task<Domain.Entities.SupplierContract?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default,
		DataFetchMode mode = DataFetchMode.ActiveOnly);

	public Task<List<Domain.Entities.SupplierContract>> GetAllAsync(
		CancellationToken cancellationToken = default,
		DataFetchMode mode = DataFetchMode.ActiveOnly);

	public Task<Domain.Entities.SupplierContract?> GetActiveContractBySupplierIdAsync(
		int supplierId,
		CancellationToken cancellationToken = default);

	public Task<List<SupplierContractAuditLog>> GetAuditLogsAsync(
		Guid supplierContractId,
		CancellationToken cancellationToken = default);

	public Task<bool> IsContractNumberExistsAsync(
		string contractNumber,
		Guid? excludeId = null,
		CancellationToken cancellationToken = default);

	public Task<int> CountAsync(CancellationToken cancellationToken = default);

	public Task<int> CountByStatusAsync(string status, CancellationToken cancellationToken = default);

	public Task<int> CountExpiringAsync(int daysThreshold, CancellationToken cancellationToken = default);
}

