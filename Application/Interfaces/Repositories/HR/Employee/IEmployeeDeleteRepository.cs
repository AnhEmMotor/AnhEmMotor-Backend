using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface IEmployeeDeleteRepository
{
    void Delete(EmployeeProfile entity);
    void Delete(IEnumerable<EmployeeProfile> entities);
}
