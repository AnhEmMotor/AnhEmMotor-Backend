using Domain.Entities;

namespace Application.Interfaces.Repositories.ConversionTool;

public interface IConversionToolReadRepository
{
    Task<List<Domain.Entities.ConversionTool>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Domain.Entities.ConversionTool?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
