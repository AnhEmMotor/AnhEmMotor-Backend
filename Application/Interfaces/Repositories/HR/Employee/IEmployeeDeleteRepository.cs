using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface IEmployeeDeleteRepository
{
    public void Delete(EmployeeProfile entity);

    public void Delete(IEnumerable<EmployeeProfile> entities);
}
