using Application.Interfaces.Repositories.WarrantyTerm;
using global::Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.WarrantyTerm;

public class WarrantyTermWriteRepository(ApplicationDBContext context) : IWarrantyTermWriteRepository
{
	public async Task AddAsync(global::Domain.Entities.WarrantyTerm entity, CancellationToken ct = default)
	{
		await context.Set<global::Domain.Entities.WarrantyTerm>().AddAsync(entity, ct).ConfigureAwait(false);
	}

	public async Task UpdateAsync(global::Domain.Entities.WarrantyTerm entity, CancellationToken ct = default)
	{
		context.Set<global::Domain.Entities.WarrantyTerm>().Update(entity);
		await Task.CompletedTask;
	}

	public async Task DeleteAsync(int id, CancellationToken ct = default)
	{
		var entity = await context
			.Set<global::Domain.Entities.WarrantyTerm>()
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(x => x.Id == id, ct)
			.ConfigureAwait(false);

		if (entity != null)
			context.Set<global::Domain.Entities.WarrantyTerm>().Remove(entity);
	}

	public async Task RestoreAsync(int id, CancellationToken ct = default)
	{
		var entity = await context
			.Set<global::Domain.Entities.WarrantyTerm>()
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(x => x.Id == id, ct)
			.ConfigureAwait(false);

		if (entity != null)
			context.Set<global::Domain.Entities.WarrantyTerm>().Update(entity);

		await Task.CompletedTask;
	}
}
