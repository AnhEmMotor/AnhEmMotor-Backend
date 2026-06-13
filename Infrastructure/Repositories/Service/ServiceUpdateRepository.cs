using Application.Interfaces.Repositories.Service;
using Infrastructure.DBContexts;
using ServiceEntity = Domain.Entities.Service;

namespace Infrastructure.Repositories.Service;

public class ServiceUpdateRepository(ApplicationDBContext context) : IServiceUpdateRepository
{
    public void Update(ServiceEntity service)
    {
        context.Services.Update(service);
    }
}
