using Domain.Entities;

namespace Application.Interfaces.Repositories.HR.Employee;

public interface IEmployeeUpdateRepository
{
    public void Update(EmployeeProfile employee);
}
