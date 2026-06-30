using Domain.Entities;

namespace Application.Interfaces.Repositories.ConversionTool;

public interface IConversionToolWriteRepository
{
    Task<ConversionTool> AddAsync(ConversionTool entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConversionTool entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ConversionTool entity, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<ConversionTool> entities, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
