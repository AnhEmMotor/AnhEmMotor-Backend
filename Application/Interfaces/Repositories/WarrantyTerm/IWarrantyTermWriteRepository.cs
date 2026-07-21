using global::Domain.Entities;

namespace Application.Interfaces.Repositories.WarrantyTerm;

public interface IWarrantyTermWriteRepository
{
	public Task AddAsync(global::Domain.Entities.WarrantyTerm entity, CancellationToken cancellationToken = default);

	public Task UpdateAsync(global::Domain.Entities.WarrantyTerm entity, CancellationToken cancellationToken = default);

	public Task DeleteAsync(int id, CancellationToken cancellationToken = default);

	public Task RestoreAsync(int id, CancellationToken cancellationToken = default);
}
