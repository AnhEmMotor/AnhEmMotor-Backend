
namespace Application.Interfaces.Repositories.ConversionTool;

public interface IConversionToolWriteRepository
{
    public Task<Domain.Entities.ConversionTool> AddAsync(
        Domain.Entities.ConversionTool entity,
        CancellationToken cancellationToken = default);

    public Task UpdateAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default);

    public Task DeleteAsync(Domain.Entities.ConversionTool entity, CancellationToken cancellationToken = default);

    public Task DeleteRangeAsync(
        IEnumerable<Domain.Entities.ConversionTool> entities,
        CancellationToken cancellationToken = default);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
