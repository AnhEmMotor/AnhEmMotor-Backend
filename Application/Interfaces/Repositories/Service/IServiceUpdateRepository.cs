using ServiceEntity = Domain.Entities.Service;

namespace Application.Interfaces.Repositories.Service
{
    public interface IServiceUpdateRepository
    {
        void Update(ServiceEntity service);
    }
}
