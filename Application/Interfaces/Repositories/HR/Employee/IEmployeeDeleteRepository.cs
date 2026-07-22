using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface IEmployeeDeleteRepository
{
    public void Delete(EmployeeProfile employee);
}
