using ServiceBookingEntity = AnhEmMotor.Domain.Entities.ServiceBooking;
using System.Threading;
using System.Threading.Tasks;

namespace AnhEmMotor.Application.Interfaces.Repositories.ServiceBooking
{
    public interface IServiceBookingInsertRepository
    {
        public Task<int> AddAsync(ServiceBookingEntity booking, CancellationToken cancellationToken);
    }
}
