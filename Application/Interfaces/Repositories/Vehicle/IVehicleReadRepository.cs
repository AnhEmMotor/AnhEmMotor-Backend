
namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehicleReadRepository
{
    public Task<List<Domain.Entities.Vehicle>> GetVehiclesAsync(
        string? search,
        CancellationToken cancellationToken = default);
}
