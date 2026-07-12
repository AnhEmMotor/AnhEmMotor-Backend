
namespace Application.Interfaces.Repositories.ConversionTool;

public interface IConversionToolReadRepository
{
    public Task<List<Domain.Entities.ConversionTool>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<Domain.Entities.ConversionTool?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
