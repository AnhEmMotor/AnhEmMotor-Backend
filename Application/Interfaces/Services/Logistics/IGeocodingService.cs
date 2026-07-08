using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Services.Logistics
{
    public interface IGeocodingService
    {
        Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address, CancellationToken cancellationToken = default);
    }
}
