using Domain.Entities;

namespace Application.Interfaces.Repositories.ConversionTool;

public interface IConversionToolWriteRepository
{
    Task<Domain.Entities.ConversionTool> AddAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<Domain.Entities.ConversionTool> entities, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
