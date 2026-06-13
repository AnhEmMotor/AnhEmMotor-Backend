using ServiceEntity = Domain.Entities.Service;

namespace Application.Interfaces.Repositories.Service
{
    public interface IServiceUpdateRepository
    {
        public void Update(ServiceEntity service);
    }
}
